using System.Threading;
using System.Threading.Tasks;

namespace Cosmos.Phantom.SDK.Seeding.Interfaces;

public interface ISeedFileReader
{
    Task<string?> ReadSeedFileAsync(string folderPath, string containerName, CancellationToken ct = default);
}
