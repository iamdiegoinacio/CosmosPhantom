# Arquitetura: Cosmos Phantom SDK

Abaixo, detalhamos a arquitetura do **Cosmos Phantom SDK** em três visões principais.

---

## 1. Visão Geral de Componentes (Diagrama de Fluxo)

Este diagrama mostra as principais peças do SDK e como elas interagem com a Aplicação Principal (Consumer API) e com o pacote base (`CosmosDB.InMemoryEmulator`).

```mermaid
flowchart TD
    subgraph ConsumerAPI ["API Consumidora (Host)"]
        Config["appsettings.json"]
        Program["Program.cs"]
        Seeds["Pasta /Seeds"]
    end

    subgraph PhantomSDK ["Cosmos Phantom SDK"]
        Options["Options Pattern Validations"]
        ExtSetup["AddCosmosPhantomEmulator"]
        ExtSeed["UseCosmosPhantomSeederAsync"]
        
        subgraph Seeding ["Motor de Seeding"]
            SeederService("CosmosDbSeederService")
            DbManager("CosmosDbManager")
            FileReader("SeedFileReader")
            BulkInserter("CosmosDbBulkInserter")
        end
        
        Chaos["ChaosEngineeringConfigurator"]
    end

    subgraph BaseEmulator ["CosmosDB.InMemoryEmulator NuGet"]
        InMemDB[("In-Memory Cosmos Server")]
        HttpMessageHandler["Custom HttpMessageHandler"]
    end

    %% Relações da inicialização
    Program -->|Configura DI| ExtSetup
    ExtSetup -->|Valida via| Options
    Options -->|Lê| Config
    
    Program -->|Executa pipeline| ExtSeed
    ExtSeed -->|Inicia| SeederService
    SeederService -->|Lê JSONs| FileReader
    FileReader -->|Busca de| Seeds
    SeederService -->|Cria DB/Containers| DbManager
    SeederService -->|Delega Inserção| BulkInserter
    BulkInserter -->|Grava dados| InMemDB
    
    %% Relações do Emulador Base
    ExtSetup -->|Injeta DelegatingHandler| Chaos
    Chaos -->|Configura Interceptador| HttpMessageHandler
    HttpMessageHandler --> InMemDB
```

---

## 2. Fluxo de Inicialização e Seeding (Sequence Diagram)

Mostra o comportamento sequencial no momento em que a aplicação "sobe", lendo os arquivos locais e populando a base na memória.

```mermaid
sequenceDiagram
    participant App as API Host (Program.cs)
    participant SDK as PhantomCosmosSeederExtensions
    participant Seeder as CosmosDbSeederService
    participant Reader as SeedFileReader
    participant Inserter as CosmosDbBulkInserter
    participant Base as InMemoryEmulator

    App->>SDK: UseCosmosPhantomSeederAsync()
    SDK->>SDK: Valida ambiente (Fail-Fast)
    SDK->>Seeder: SeedAsync(seedsFolderPath)
    
    Seeder->>Base: Cria Banco de Dados
    
    loop Para cada Container no Config
        Seeder->>Base: Cria Container
        Seeder->>Reader: ReadSeedFileAsync(folder, container)
        Reader-->>Seeder: Retorna JSON string
        Seeder->>Inserter: BulkInsertAsync(container, json)
        Inserter->>Inserter: Parse do JSON (Lotes de 50)
        Inserter->>Base: UpsertItemAsync (em concorrência)
        Inserter-->>Seeder: Retorna quantidade inserida
    end
    
    Seeder-->>SDK: Concluído
    SDK-->>App: Pipeline liberado (API pronta para requisições)
```

---

## 3. Fluxo em Tempo de Execução e Chaos Engineering

Mostra o que acontece quando a API já está de pé e um repositório da aplicação tenta acessar o banco de dados. O Chaos Engineering atua aqui, interceptando as requisições e injetando falhas para testar a resiliência.

```mermaid
sequenceDiagram
    participant Repo as Application Repository
    participant CosmosClient as CosmosClient (Mocked)
    participant Chaos as ChaosEngineeringConfigurator
    participant InMem as Banco em Memória

    Repo->>CosmosClient: GetItemQueryIterator<T>() / CreateItemAsync()
    CosmosClient->>Chaos: Requisição HTTP Interna (Mock)
    
    alt Se Chaos Engineering NÃO simular falha
        Chaos->>InMem: Repassa requisição
        InMem-->>Chaos: Retorna Dados (200 OK / 201 Created)
        Chaos-->>CosmosClient: Retorna Dados
        CosmosClient-->>Repo: Sucesso
    else Se Chaos Engineering SIMULAR falha (ex: 429)
        Chaos-->>CosmosClient: Aborta e retorna HTTP Status 429 Too Many Requests
        CosmosClient-->>CosmosClient: Aciona Política de Retry Nativa (se ativada)
        CosmosClient-->>Repo: Lança CosmosException (StatusCode 429) para a aplicação tratar
    end
```
