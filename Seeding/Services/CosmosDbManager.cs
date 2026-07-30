using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;
using Cosmos.Phantom.InMemoryEmulator.SDK.Configuration;
using Cosmos.Phantom.InMemoryEmulator.SDK.Seeding.Interfaces;
using System.Collections.ObjectModel;

namespace Cosmos.Phantom.InMemoryEmulator.SDK.Seeding.Services;

public class CosmosDbManager : ICosmosDbManager
{
    private readonly CosmosClient _cosmosClient;

    public CosmosDbManager(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    public async Task<Database> CreateDatabaseIfNotExistsAsync(string databaseName)
    {
        var response = await _cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);
        return response.Database;
    }

    public async Task<Container> CreateContainerIfNotExistsAsync(Database database, ContainerConfig containerConfig)
    {
        var containerProperties = containerConfig.ToContainerProperties();
        var response = await database.CreateContainerIfNotExistsAsync(containerProperties);
        return response.Container;
    }
}
