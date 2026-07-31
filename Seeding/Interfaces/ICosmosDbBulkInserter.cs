using Microsoft.Azure.Cosmos;
using System.Threading;
using System.Threading.Tasks;
using Cosmos.Phantom.SDK.Configuration;

namespace Cosmos.Phantom.SDK.Seeding.Interfaces;

public interface ICosmosDbBulkInserter
{
    Task<int> BulkInsertAsync(Container container, ContainerConfig containerConfig, string jsonContent, CancellationToken ct);
}
