using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;
using Cosmos.Phantom.SDK.Configuration;
using Cosmos.Phantom.SDK.Seeding.Interfaces;
using System.Collections.ObjectModel;

namespace Cosmos.Phantom.SDK.Seeding.Services;

public class CosmosDbManager(CosmosClient cosmosClient) : ICosmosDbManager
{

    public async Task<Database> CreateDatabaseIfNotExistsAsync(string databaseName)
    {
        var response = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);
        return response.Database;
    }

    public async Task<Container> CreateContainerIfNotExistsAsync(Database database, ContainerConfig containerConfig)
    {
        var containerProperties = containerConfig.ToContainerProperties();
        var response = await database.CreateContainerIfNotExistsAsync(containerProperties);
        return response.Container;
    }
}
