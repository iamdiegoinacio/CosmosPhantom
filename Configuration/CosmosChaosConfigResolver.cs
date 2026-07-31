using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Cosmos.Phantom.SDK.ChaosEngineering;

namespace Cosmos.Phantom.SDK.Configuration;

internal static class CosmosChaosConfigResolver
{
    /// <summary>
    /// Lê a configuração providenciada pelo consumidor e realiza o fallback/merge 
    /// com a configuração padrão de caos embutida no SDK.
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
            // Mescla de configurações: se o usuário preencheu o section mas não definiu tudo
            // Poderíamos fazer um merge detalhado aqui se houvesse propriedades complexas aninhadas,
            // mas como o ChaosConfig são apenas booleanos e doubles primitivos, 
            // e tipos primitivos têm valor default (false/0.0), é difícil saber se o usuário
            // os declarou explicitamente como false ou se foram omitidos.
            // Para simplificar, consideramos que se ele criou a sessão "CosmosChaosEngineering",
            // ele está assumindo o controle total. Caso contrário, usamos o fallback.
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
            return null; // Falha segura se o embedded resource não for encontrado
        }
    }
}
