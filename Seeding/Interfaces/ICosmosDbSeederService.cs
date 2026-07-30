using System.Threading;
using System.Threading.Tasks;
using Cosmos.Phantom.InMemoryEmulator.SDK.Configuration;

namespace Cosmos.Phantom.InMemoryEmulator.SDK.Seeding.Interfaces;

public interface ICosmosDbSeederService
{
    Task SeedAsync(string seedsFolderPath, CancellationToken ct = default);
}
