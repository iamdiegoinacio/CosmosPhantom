using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cosmos.Phantom.InMemoryEmulator.SDK.Seeding.Interfaces;

namespace Cosmos.Phantom.InMemoryEmulator.SDK.Seeding.Services;

public class SeedFileReader : ISeedFileReader
{
    public async Task<string> ReadSeedFileAsync(string folderPath, string containerName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(folderPath) || string.IsNullOrWhiteSpace(containerName))
            return null;

        var seedFilePath = Path.Combine(folderPath, $"{containerName}.json");
        
        if (!File.Exists(seedFilePath))
        {
            return null;
        }

        return await File.ReadAllTextAsync(seedFilePath, ct);
    }
}
