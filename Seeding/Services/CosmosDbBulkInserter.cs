using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Cosmos.Phantom.SDK.Configuration;
using Cosmos.Phantom.SDK.Exceptions;
using Cosmos.Phantom.SDK.Seeding.Interfaces;

namespace Cosmos.Phantom.SDK.Seeding.Services;

public class CosmosDbBulkInserter(ILogger<CosmosDbBulkInserter> logger) : ICosmosDbBulkInserter
{

    public async Task<int> BulkInsertAsync(Container container, ContainerConfig containerConfig, string jsonContent, CancellationToken ct)
    {
        JArray jArray;
        try
        {
            jArray = JArray.Parse(jsonContent);
        }
        catch (Exception ex)
        {
            throw new CosmosDbEmulatorSeedingException(
                $"Falha crítica ao realizar o parse do arquivo de seed para o container '{containerConfig.Name}'. O arquivo contém um JSON inválido.", ex);
        }

        if (jArray == null)
        {
            logger.LogWarning("[CosmosDbSeeder] O arquivo de seed do container '{Container}' está vazio ou não é um array válido.", containerConfig.Name);
            return 0;
        }

        int count = 0;
        string partitionKeyPropName = containerConfig.PartitionKeyPath.TrimStart('/');
        var tasks = new List<Task>();

        foreach (var item in jArray.OfType<JObject>())
        {
            string pkValue = item[partitionKeyPropName]?.ToString();

            if (string.IsNullOrEmpty(pkValue))
            {
                logger.LogWarning("[CosmosDbSeeder] Objeto sem a propriedade de PK '{PkName}' no container '{Container}'. Pulando...", partitionKeyPropName, containerConfig.Name);
                continue;
            }

            tasks.Add(container.CreateItemAsync<dynamic>(item, new PartitionKey(pkValue), null, ct)
                .ContinueWith(t => 
                {
                    if (t.IsCompletedSuccessfully)
                        Interlocked.Increment(ref count);
                    else
                        logger.LogError(t.Exception, "[CosmosDbSeeder] Falha ao inserir item com PK {PkValue}", pkValue);
                }, ct));

            // Lotes de 50 inserÃ§Ãµes assÃ­ncronas concorrentes
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

        logger.LogInformation("[CosmosDbSeeder] {Count} itens inseridos no container '{Container}'.", count, containerConfig.Name);
        
        return count;
    }
}
