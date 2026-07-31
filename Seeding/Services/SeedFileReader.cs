using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Cosmos.Phantom.SDK.Seeding.Interfaces;

namespace Cosmos.Phantom.SDK.Seeding.Services;

public class SeedFileReader : ISeedFileReader
{
    public async Task<string?> ReadSeedFileAsync(string folderPath, string containerName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(containerName))
            return null;

        // 1. Tenta ler do disco do consumidor
        if (!string.IsNullOrWhiteSpace(folderPath))
        {
            var seedFilePath = Path.Combine(folderPath, $"{containerName}.json");
            
            if (File.Exists(seedFilePath))
            {
                return await File.ReadAllTextAsync(seedFilePath, ct);
            }
        }

        // 2. Fallback: Tenta ler o arquivo padrÃ£o embutido na DLL do SDK
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"Cosmos.Phantom.SDK.Resources.Seeds.{containerName}.json";
        
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream != null)
        {
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync(ct);
        }

        return null;
    }
}
