using System.Threading;
using System.Threading.Tasks;
using Cosmos.Phantom.SDK.Configuration;

namespace Cosmos.Phantom.SDK.Seeding.Interfaces;

public interface ICosmosDbSeederService
{
    Task SeedAsync(string seedsFolderPath, CancellationToken ct = default);
}
