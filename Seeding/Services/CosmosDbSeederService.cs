using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Options;
using Cosmos.Phantom.InMemoryEmulator.SDK.Configuration;
using Cosmos.Phantom.InMemoryEmulator.SDK.Exceptions;
using Cosmos.Phantom.InMemoryEmulator.SDK.Seeding.Interfaces;

namespace Cosmos.Phantom.InMemoryEmulator.SDK.Seeding.Services;

public class CosmosDbSeederService : ICosmosDbSeederService
{
    private readonly ICosmosDbManager _dbManager;
    private readonly ISeedFileReader _fileReader;
    private readonly ILogger<CosmosDbSeederService> _logger;
    private readonly CosmosDbEmulatorConfig _config;

    public CosmosDbSeederService(
        ICosmosDbManager dbManager, 
        ISeedFileReader fileReader, 
        ILogger<CosmosDbSeederService> logger,
        IOptions<CosmosDbEmulatorConfig> options)
    {
        _dbManager = dbManager;
        _fileReader = fileReader;
        _logger = logger;
        _config = options.Value;
    }

    public async Task SeedAsync(string seedsFolderPath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_config?.DatabaseName))
        {
            _logger.LogError("[CosmosDbSeeder] ConfiguraÃ§Ã£o invÃ¡lida ou nome do banco nÃ£o definido.");
            return;
        }

        var database = await _dbManager.CreateDatabaseIfNotExistsAsync(_config.DatabaseName);

        foreach (var containerConfig in _config.Containers)
        {
            ct.ThrowIfCancellationRequested();
            var container = await _dbManager.CreateContainerIfNotExistsAsync(database, containerConfig);
            var jsonContent = await _fileReader.ReadSeedFileAsync(seedsFolderPath, containerConfig.Name, ct);
            
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                _logger.LogWarning("[CosmosDbSeeder] Arquivo de seed nÃ£o encontrado ou vazio para '{Container}'. Criado vazio.", containerConfig.Name);
                continue;
            }

            await SeedContainerItemsAsync(container, containerConfig, jsonContent, ct);
        }
    }

    private async Task SeedContainerItemsAsync(Container container, ContainerConfig containerConfig, string jsonContent, CancellationToken ct)
    {
        JArray jArray;
        try
        {
            jArray = JArray.Parse(jsonContent);
        }
        catch (Exception ex)
        {
            throw new CosmosDbEmulatorSeedingException(
                $"Falha crÃ­tica ao realizar o parse do arquivo de seed para o container '{containerConfig.Name}'. O arquivo contÃ©m um JSON invÃ¡lido.", ex);
        }

        if (jArray == null)
        {
            _logger.LogWarning("[CosmosDbSeeder] O arquivo de seed do container '{Container}' estÃ¡ vazio ou nÃ£o Ã© um array vÃ¡lido.", containerConfig.Name);
            return;
        }

        int count = 0;
        string partitionKeyPropName = containerConfig.PartitionKeyPath.TrimStart('/');
        var tasks = new List<Task>();

        foreach (var item in jArray.OfType<JObject>())
        {
            string pkValue = item[partitionKeyPropName]?.ToString();

            if (string.IsNullOrEmpty(pkValue))
            {
                _logger.LogWarning("[CosmosDbSeeder] Objeto sem a propriedade de PK '{PkName}' no container '{Container}'. Pulando...", partitionKeyPropName, containerConfig.Name);
                continue;
            }

            tasks.Add(container.CreateItemAsync<dynamic>(item, new PartitionKey(pkValue), null, ct)
                .ContinueWith(t => 
                {
                    if (t.IsCompletedSuccessfully)
                        Interlocked.Increment(ref count);
                    else
                        _logger.LogError(t.Exception, "[CosmosDbSeeder] Falha ao inserir item com PK {PkValue}", pkValue);
                }, ct));

            // Lotes de 50 inserÃ§Ãµes assÃ­ncronas concorrentes para nÃ£o travar a Startup
            if (tasks.Count >= 50)
            {
                await Task.WhenAll(tasks);
                tasks.Clear();
            }
        }

        if (tasks.Count > 0)
        {
            await Task.WhenAll(tasks);
        }

        _logger.LogInformation("[CosmosDbSeeder] {Count} itens inseridos no container '{Container}'.", count, containerConfig.Name);
    }
}
