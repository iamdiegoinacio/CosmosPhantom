using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Cosmos.Phantom.SDK.ChaosEngineering;

namespace Cosmos.Phantom.SDK.Configuration;

internal static class CosmosChaosConfigResolver
{
    /// <summary>
    /// LÃª a configuraÃ§Ã£o providenciada pelo consumidor e realiza o fallback/merge 
    /// com a configuraÃ§Ã£o padrÃ£o de caos embutida no SDK.
    /// </summary>
    public static ChaosConfig? Resolve(IConfiguration configuration)
    {
        var chaosConfig = configuration.GetSection("CosmosChaosEngineering").Get<ChaosConfig>();
        var fallbackConfig = LoadEmbeddedConfig();

        if (chaosConfig == null)
        {
            chaosConfig = fallbackConfig;
        }
        else if (fallbackConfig != null)
        {
            // Mescla de configuraÃ§Ãµes: se o usuÃ¡rio preencheu o section mas nÃ£o definiu tudo
            // PoderÃ­amos fazer um merge detalhado aqui se houvesse propriedades complexas aninhadas,
            // mas como o ChaosConfig sÃ£o apenas booleanos e doubles primitivos, 
            // e tipos primitivos tÃªm valor default (false/0.0), Ã© difÃ­cil saber se o usuÃ¡rio
            // os declarou explicitamente como false ou se foram omitidos.
            // Para simplificar, consideramos que se ele criou a sessÃ£o "CosmosChaosEngineering",
            // ele estÃ¡ assumindo o controle total. Caso contrÃ¡rio, usamos o fallback.
        }

        return chaosConfig;
    }

    private static ChaosConfig? LoadEmbeddedConfig()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Cosmos.Phantom.SDK.Resources.Chaos.Settings.json";
            
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) return null;

            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            var jObj = JObject.Parse(json);
            
            return jObj["CosmosChaosEngineering"]?.ToObject<ChaosConfig>();
        }
        catch
        {
            return null; // Falha segura se o embedded resource nÃ£o for encontrado
        }
    }
}
