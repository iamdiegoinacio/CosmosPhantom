using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;
using Cosmos.Phantom.SDK.Configuration;

namespace Cosmos.Phantom.SDK.Seeding.Interfaces;

public interface ICosmosDbManager
{
    Task<Database> CreateDatabaseIfNotExistsAsync(string databaseName);
    Task<Container> CreateContainerIfNotExistsAsync(Database database, ContainerConfig containerConfig);
}
