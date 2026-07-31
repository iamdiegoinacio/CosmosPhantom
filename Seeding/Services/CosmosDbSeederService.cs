using Microsoft.Azure.Cosmos;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Cosmos.Phantom.SDK.Configuration;
using Cosmos.Phantom.SDK.Seeding.Interfaces;

namespace Cosmos.Phantom.SDK.Seeding.Services;

public class CosmosDbSeederService(
    ICosmosDbManager dbManager, 
    ISeedFileReader fileReader, 
    ICosmosDbBulkInserter bulkInserter,
    ILogger<CosmosDbSeederService> logger,
    IOptions<CosmosDbEmulatorConfig> options) : ICosmosDbSeederService
{
    private readonly CosmosDbEmulatorConfig _config = options.Value;

    public async Task SeedAsync(string seedsFolderPath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_config?.DatabaseName))
        {
            logger.LogError("[CosmosDbSeeder] ConfiguraÃ§Ã£o invÃ¡lida ou nome do banco nÃ£o definido.");
            return;
        }

        var database = await dbManager.CreateDatabaseIfNotExistsAsync(_config.DatabaseName);

        foreach (var containerConfig in _config.Containers)
        {
            ct.ThrowIfCancellationRequested();
            var container = await dbManager.CreateContainerIfNotExistsAsync(database, containerConfig);
            var jsonContent = await fileReader.ReadSeedFileAsync(seedsFolderPath, containerConfig.Name, ct);
            
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                logger.LogWarning("[CosmosDbSeeder] Arquivo de seed nÃ£o encontrado ou vazio para '{Container}'. Criado vazio.", containerConfig.Name);
                continue;
            }

            await bulkInserter.BulkInsertAsync(container, containerConfig, jsonContent, ct);
        }
    }
}
