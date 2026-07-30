<div align="center">
  <img src="Docs/Img/phantom_logo.png" alt="Cosmos Phantom Logo" width="250"/>
</div>

# Cosmos Phantom SDK (InMemoryEmulator)

![.NET 8](https://img.shields.io/badge/.NET-8.0-blueviolet.svg)
![Conventional Commits](https://img.shields.io/badge/Conventional%20Commits-1.0.0-%23FE5196?logo=conventionalcommits&logoColor=white)
![License](https://img.shields.io/badge/license-MIT-green)

**Cosmos Phantom SDK** é um pacote corporativo para .NET focado em fornecer um ambiente de banco de dados Cosmos DB em memória, de forma nativa e aderente aos padrões da Microsoft. Ele facilita o desenvolvimento local, a automação de testes e a engenharia do caos, sem a necessidade de infraestrutura pesada ou acesso a recursos em nuvem reais durante o ciclo de desenvolvimento.

## Principais Recursos

- **Mock de Cosmos DB Em Memória:** Substitui o `CosmosClient` em ambiente de desenvolvimento por um mock leve em memória, sem poluir código de produção.
- **Auto-Seeding (Carga de Dados):** Lê automaticamente arquivos JSON de um diretório e popula as coleções com dados de teste assim que a aplicação sobe.
- **Chaos Engineering Embutido:** Permite injetar falhas aleatórias (throttling, timeouts, erros HTTP variados) direto pela configuração para testar a resiliência da aplicação.
- **Configuração Validada (Fail-Fast):** O SDK valida suas configurações através do "Options Pattern", parando a execução imediatamente se as definições do ambiente não estiverem corretas.
- **Pluguabilidade:** Com apenas duas linhas no seu `Program.cs`, o SDK ativa ou desativa toda a infraestrutura baseada nas propriedades de ambiente.

---

## 1. Instalação e Pré-requisitos

Para que o SDK Phantom consiga ler seus arquivos de configuração e sementes (seeds) durante a execução, o projeto consumidor (ex: `Host.csproj`) precisa obrigatoriamente referenciar o SDK e garantir que os arquivos `.json` sejam copiados para a pasta de saída (Output Directory).

Adicione o seguinte trecho no `.csproj` da sua API:

```xml
<ItemGroup>
    <!-- Referência e Arquivos do Phantom SDK -->
    <ProjectReference Include="..\Cosmos.Phantom.InMemoryEmulator.SDK\Cosmos.Phantom.InMemoryEmulator.SDK.csproj" />
    <Content Update="Cosmos.Emulator.Config.json">
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Update="Seeds\*.json">
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
</ItemGroup>
```

---

## 2. Como Usar no `Program.cs`

No seu `Program.cs`, certifique-se de importar os *namespaces* obrigatórios para acessar os métodos de extensão do SDK:

```csharp
using Cosmos.Phantom.InMemoryEmulator.SDK;
using Cosmos.Phantom.InMemoryEmulator.SDK.Seeding;
// using Cosmos.Phantom.InMemoryEmulator.SDK.Configuration; (Opcional)
```

A injeção do SDK foi projetada para ser simples e seguir as práticas idiomáticas do ASP.NET Core:

```csharp
// 1. Fase de Configuração de Serviços (builder.Services)
// Adiciona o SDK de emulação e o substitui o client Cosmos na injeção de dependências.
builder.Services.AddCosmosDbEmulator(builder.Environment, builder.Configuration);

var app = builder.Build();

// 2. Fase de Pipeline / Middleware
// Executa o Seeder para popular os dados antes da API aceitar as requisições.
await app.Services.UseCosmosDbEmulatorSeederAsync();

app.Run();
```

*Nota: O SDK é inteligente o bastante para não executar ou injetar nada se o ambiente não for `Development` ou se a flag `UseCosmosDbEmulator` for `false`.*

---

## 3. Configurações (`appsettings.json`)

Para que o SDK funcione, adicione a seção `CosmosDbEmulator` nas suas configurações. Caso algo obrigatório falte, a API recusará a inicialização (Fail-Fast).

```json
{
  "UseCosmosDbEmulator": true,
  "CosmosDbEmulator": {
    "DatabaseName": "MeuBancoLocal",
    "Containers": [
      {
        "Name": "Usuarios",
        "PartitionKeyPath": "/id"
      },
      {
        "Name": "Produtos",
        "PartitionKeyPath": "/categoria"
      }
    ],
    "Chaos": {
      "EnableThrottlingMode": false,
      "ThrottlingRate": 0.2,
      "Simulate429_TooManyRequests": false,
      "Simulate503_ServiceUnavailable": false,
      "Simulate408_RequestTimeout": false,
      "Simulate400_BadRequest": false
    }
  }
}
```

### Detalhamento das Propriedades

- **`DatabaseName`**: (Obrigatório) Nome do banco de dados simulado.
- **`Containers`**: (Obrigatório) Lista das coleções a serem criadas. O nome é sensível a maiúsculas/minúsculas e será usado para buscar os dados de semente (Seeds).
- **`PartitionKeyPath`**: (Obrigatório) Caminho da chave de partição. Default é `/id`.
- **`Chaos`**: (Opcional) Configuração das simulações de anomalias (Engenharia do Caos).

---

## 4. Auto-Seeding (Carga de Dados)

O SDK tentará automaticamente buscar os arquivos base para os containers na pasta `Seeds` localizada na raiz onde o projeto principal (API Host) for executado.

### Como funciona:
1. Crie uma pasta chamada `Seeds` na raiz do projeto final.
2. Adicione arquivos JSON com exatamente o mesmo nome dos seus `Containers` (ex: `Usuarios.json`, `Produtos.json`).
3. O conteúdo do JSON **deve ser um array de objetos válidos** e deve conter a propriedade que representa a Partition Key do container.

**Exemplo - `Seeds/Usuarios.json`:**
```json
[
  {
    "id": "1e5509ba-1c25-412e-990a-a5bb9b674b88",
    "nome": "Usuário Teste",
    "email": "teste@exemplo.com"
  },
  {
    "id": "59b4ef54-8c08-41d3-9824-3453b53fc065",
    "nome": "Admin",
    "email": "admin@exemplo.com"
  }
]
```

O *Seeder* agrupa as inserções (via Task.WhenAll) em lotes (batching) garantindo inicialização rápida, e avisa silenciosamente por logs do sistema caso falte algum arquivo ou a modelagem esteja errada.

---

## 5. Chaos Engineering (Testes de Resiliência)

O Chaos Engineering Configurator permite injetar falhas no SDK, simulando condições adversas sem alterar nenhuma linha de código na aplicação principal. Ideal para validar se as suas políticas de Retry e Fallback (Polly) estão funcionando.

No arquivo de configuração, apenas mude as propriedades booleanas para `true` para simular as panes, por exemplo:

```json
"Chaos": {
    "EnableThrottlingMode": true,
    "ThrottlingRate": 0.5, 
    "Simulate429_TooManyRequests": true
}
```
Isso forçará o Emulador a rejeitar aleatoriamente 50% das requisições devolvendo um sub-status Cosmos de `429 Too Many Requests`.

As seguintes anomalias HTTP são suportadas no Chaos SDK:
- `Simulate429_TooManyRequests` (Gera Retry automático via Cosmos SDK se não houver política customizada)
- `Simulate503_ServiceUnavailable`
- `Simulate408_RequestTimeout`
- `Simulate403_Forbidden`
- `Simulate401_Unauthorized`
- `Simulate409_Conflict`
- `Simulate413_EntityTooLarge`
- `Simulate412_PreconditionFailed`
- `Simulate400_BadRequest`

---

## Arquitetura

O SDK adota os princípios **SOLID**:
- Configuração suportada pelo **Options Pattern** nativo com `[Required]` DataAnnotations.
- Delegação de tarefas via interfaces injetáveis (`ICosmosDbSeederService`, `ICosmosDbManager`).
- Manipulação da injeção de dependência isolada, evitando poluir o WebApplication Builder do software principal.

---