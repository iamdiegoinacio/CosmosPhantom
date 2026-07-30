using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;
using Cosmos.Phantom.InMemoryEmulator.SDK.Configuration;

namespace Cosmos.Phantom.InMemoryEmulator.SDK.Seeding.Interfaces;

public interface ICosmosDbManager
{
    Task<Database> CreateDatabaseIfNotExistsAsync(string databaseName);
    Task<Container> CreateContainerIfNotExistsAsync(Database database, ContainerConfig containerConfig);
}
