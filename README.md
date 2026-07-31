<div align="center">
  <img src="Docs/Img/phantom_logo.png" alt="Cosmos Phantom Logo" width="250"/>
</div>

# Cosmos Phantom SDK (InMemoryEmulator)

![.NET 8](https://img.shields.io/badge/.NET-8.0-blueviolet.svg)
![Conventional Commits](https://img.shields.io/badge/Conventional%20Commits-1.0.0-%23FE5196?logo=conventionalcommits&logoColor=white)
![License](https://img.shields.io/badge/license-MIT-green)

**Cosmos Phantom SDK** Ã© um SDK para .NET focado em fornecer um ambiente de banco de dados Cosmos DB em memÃ³ria, de forma nativa e aderente aos padrÃµes da Microsoft. Ele facilita o desenvolvimento local, a automaÃ§Ã£o de testes e a engenharia do caos, sem a necessidade de infraestrutura pesada ou acesso a recursos em nuvem reais durante o ciclo de desenvolvimento.

## Principais Recursos

- **Mock de Cosmos DB Em MemÃ³ria:** Substitui o `CosmosClient` em ambiente de desenvolvimento por um mock leve em memÃ³ria, sem poluir cÃ³digo de produÃ§Ã£o.
- **Auto-Seeding (Carga de Dados):** LÃª automaticamente arquivos JSON de um diretÃ³rio e popula as coleÃ§Ãµes com dados de teste assim que a aplicaÃ§Ã£o sobe.
- **Chaos Engineering Embutido:** Permite injetar falhas aleatÃ³rias (throttling, timeouts, erros HTTP variados) direto pela configuraÃ§Ã£o para testar a resiliÃªncia da aplicaÃ§Ã£o.
- **ConfiguraÃ§Ã£o Validada (Fail-Fast):** O SDK valida suas configuraÃ§Ãµes atravÃ©s do "Options Pattern", parando a execuÃ§Ã£o imediatamente se as definiÃ§Ãµes do ambiente nÃ£o estiverem corretas.
- **Pluguabilidade:** Com apenas duas linhas no seu `Program.cs`, o SDK ativa ou desativa toda a infraestrutura baseada nas propriedades de ambiente.

---

## CrÃ©ditos e Base TecnolÃ³gica

Este SDK foi construÃ­do tendo como motor principal o pacote NuGet **CosmosDB.InMemoryEmulator**.

```xml
<PackageReference Include="CosmosDB.InMemoryEmulator" Version="4.0.20" />
```

**O que o Cosmos Phantom SDK faz com esse pacote?**
O pacote `CosmosDB.InMemoryEmulator` provÃª a fundaÃ§Ã£o tÃ©cnica essencial para criar um servidor Cosmos DB local e em memÃ³ria no .NET. No entanto, o **Cosmos Phantom SDK** atua como uma camada superior (*wrapper/framework*) que estende as capacidades bÃ¡sicas. O Phantom pega o motor base do emulador e adiciona de forma transparente:

1. **Auto-seeding DinÃ¢mico:** Automatiza a injeÃ§Ã£o inicial de dados lendo nativamente arquivos `.json`, poupando a necessidade de scripts de carga manuais ao subir o emulador.
2. **Engenharia do Caos (Chaos Engineering):** Introduz uma camada de interceptaÃ§Ã£o que simula falhas de rede e de requisiÃ§Ãµes de forma sistÃªmica e configurÃ¡vel.
3. **Fail-Fast Configuration:** Adiciona validaÃ§Ãµes robustas baseadas no modelo *Options Pattern* para que o ambiente local falhe instantaneamente caso falte alguma configuraÃ§Ã£o essencial, prevenindo ambiguidades no ambiente de desenvolvimento.
4. **IntegraÃ§Ã£o Plug-and-Play no ASP.NET Core:** Simplifica o *setup* para apenas poucas linhas no `Program.cs`, abstraindo a complexidade de instanciar e gerenciar o ciclo de vida do emulador base.

---

## 1. InstalaÃ§Ã£o Simples (Plug and Play)

Para usar o SDK, o projeto consumidor (ex: `Host.csproj`) precisa apenas referenciar o SDK e ter a chave de ativaÃ§Ã£o no `appsettings.json`. O SDK jÃ¡ vem com uma **modelagem e sementes (seeds) padrÃ£o embutidas (Boilerplate)**, entÃ£o vocÃª nÃ£o precisa criar nenhum arquivo para testar!

Adicione o seguinte trecho no `.csproj` da sua API:

```xml
<ItemGroup>
    <ProjectReference Include="..\Cosmos.Phantom.SDK\Cosmos.Phantom.SDK.csproj" />
</ItemGroup>
```

---

## 2. Como Usar no `Program.cs`

No seu `Program.cs`, importe os namespaces:

```csharp
using Cosmos.Phantom.SDK;
using Cosmos.Phantom.SDK.Seeding;
```

A injeÃ§Ã£o do SDK foi projetada para ser extremamente limpa:

```csharp
// 1. Fase de ConfiguraÃ§Ã£o de ServiÃ§os
// Adiciona o SDK de emulaÃ§Ã£o injetando a configuraÃ§Ã£o padrÃ£o (ou a sua customizada)
builder.Services.AddCosmosPhantomEmulator(builder.Environment, builder.Configuration);

var app = builder.Build();

// 2. Fase de Pipeline / Middleware
// Executa o Seeder para popular os dados antes da API aceitar as requisiÃ§Ãµes
await app.Services.UseCosmosPhantomSeederAsync();

app.Run();
```

*Nota: O SDK nÃ£o injetarÃ¡ nada se o ambiente nÃ£o for `Development` ou se a flag `UseCosmosDbEmulator` for `false`.*

---

## 3. ConfiguraÃ§Ãµes e CustomizaÃ§Ãµes (Opcional)

O SDK usa uma estratÃ©gia de **Fallback**. Isso significa que ele lÃª suas prÃ³prias configuraÃ§Ãµes internas por padrÃ£o, mas **vocÃª pode sobrescrevÃª-las** quando quiser personalizar os containers e injetar o caos (Chaos Engineering).

### 3.1 Chave de AtivaÃ§Ã£o (ObrigatÃ³ria)

No seu `appsettings.Development.json`, adicione apenas a chave global para ativar o emulador:

```json
{
  "UseCosmosDbEmulator": true
}
```

### 3.2 Sobrescrevendo a ConfiguraÃ§Ã£o PadrÃ£o (`Cosmos.Phantom.Settings.json`)

Se vocÃª quiser usar uma modelagem de banco diferente da que vem embutida, basta criar um arquivo `Cosmos.Phantom.Settings.json` na raiz da sua API, referenciÃ¡-lo no `Program.cs` (`builder.Configuration.AddJsonFile("Cosmos.Phantom.Settings.json")`) e garantir que ele seja copiado no `.csproj` (`CopyToOutputDirectory`).

```json
{
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

- **`DatabaseName`**: (ObrigatÃ³rio) Nome do banco de dados simulado.
- **`Containers`**: (ObrigatÃ³rio) Lista das coleÃ§Ãµes a serem criadas. O nome Ã© sensÃ­vel a maiÃºsculas/minÃºsculas e serÃ¡ usado para buscar os dados de semente (Seeds).
- **`PartitionKeyPath`**: (ObrigatÃ³rio) Caminho da chave de partiÃ§Ã£o. Default Ã© `/id`.
- **`Chaos`**: (Opcional) ConfiguraÃ§Ã£o das simulaÃ§Ãµes de anomalias (Engenharia do Caos).

---

## 4. Auto-Seeding (Carga de Dados)

O SDK tentarÃ¡ automaticamente buscar os arquivos base para os containers na pasta `Seeds` localizada na raiz onde o projeto principal (API Host) for executado.

### Como funciona:
1. Crie uma pasta chamada `Seeds` na raiz do projeto final.
2. Adicione arquivos JSON com exatamente o mesmo nome dos seus `Containers` (ex: `Usuarios.json`, `Produtos.json`).
3. O conteÃºdo do JSON **deve ser um array de objetos vÃ¡lidos** e deve conter a propriedade que representa a Partition Key do container.

**Exemplo - `Seeds/Usuarios.json`:**
```json
[
  {
    "id": "1e5509ba-1c25-412e-990a-a5bb9b674b88",
    "nome": "UsuÃ¡rio Teste",
    "email": "teste@exemplo.com"
  },
  {
    "id": "59b4ef54-8c08-41d3-9824-3453b53fc065",
    "nome": "Admin",
    "email": "admin@exemplo.com"
  }
]
```

O *Seeder* agrupa as inserÃ§Ãµes (via Task.WhenAll) em lotes (batching) garantindo inicializaÃ§Ã£o rÃ¡pida, e avisa silenciosamente por logs do sistema caso falte algum arquivo ou a modelagem esteja errada.

---

## 5. Chaos Engineering (Testes de ResiliÃªncia)

O Chaos Engineering Configurator permite injetar falhas no SDK, simulando condiÃ§Ãµes adversas sem alterar nenhuma linha de cÃ³digo na aplicaÃ§Ã£o principal. Ideal para validar se as suas polÃ­ticas de Retry e Fallback (Polly) estÃ£o funcionando.

No arquivo de configuraÃ§Ã£o, apenas mude as propriedades booleanas para `true` para simular as panes, por exemplo:

```json
"Chaos": {
    "EnableThrottlingMode": true,
    "ThrottlingRate": 0.5, 
    "Simulate429_TooManyRequests": true
}
```
Isso forÃ§arÃ¡ o Emulador a rejeitar aleatoriamente 50% das requisiÃ§Ãµes devolvendo um sub-status Cosmos de `429 Too Many Requests`.

As seguintes anomalias HTTP sÃ£o suportadas no Chaos SDK:
- `Simulate429_TooManyRequests` (Gera Retry automÃ¡tico via Cosmos SDK se nÃ£o houver polÃ­tica customizada)
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

O SDK adota os princÃ­pios **SOLID**:
- ConfiguraÃ§Ã£o suportada pelo **Options Pattern** nativo com `[Required]` DataAnnotations.
- DelegaÃ§Ã£o de tarefas via interfaces injetÃ¡veis (`ICosmosDbSeederService`, `ICosmosDbManager`).
- ManipulaÃ§Ã£o da injeÃ§Ã£o de dependÃªncia isolada, evitando poluir o WebApplication Builder do software principal.

ðŸ‘‰ **[Ver Diagramas de Arquitetura Completos (Fluxo e SequÃªncia)](Docs/Architecture.md)**

---