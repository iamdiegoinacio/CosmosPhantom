using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Cosmos.Phantom.SDK.Configuration;

internal static class CosmosEmulatorConfigResolver
{
    /// <summary>
    /// Lê a configuração providenciada pelo consumidor e realiza o fallback/merge 
    /// com a configuração padrão embutida no SDK.
    /// </summary>
    public static CosmosDbEmulatorConfig? Resolve(IConfiguration configuration)
    {
        var emulatorConfig = configuration.GetSection("CosmosDbEmulator").Get<CosmosDbEmulatorConfig>();
        var fallbackConfig = LoadEmbeddedConfig();

        if (emulatorConfig == null)
        {
            emulatorConfig = fallbackConfig;
        }
        else if (fallbackConfig != null)
        {
            // Mescla as configurações caso o usuário tenha fornecido apenas algumas propriedades
            emulatorConfig.DatabaseName ??= fallbackConfig.DatabaseName;
            emulatorConfig.Containers ??= fallbackConfig.Containers;
        }

        return emulatorConfig;
    }

    private static CosmosDbEmulatorConfig? LoadEmbeddedConfig()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Cosmos.Phantom.SDK.Resources.Cosmos.Phantom.Settings.json";
            
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) return null;

            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            var jObj = JObject.Parse(json);
            
            return jObj["CosmosDbEmulator"]?.ToObject<CosmosDbEmulatorConfig>();
        }
        catch
        {
            return null; // Falha segura se o embedded resource não for encontrado
        }
    }
}
