using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Cosmos.Phantom.InMemoryEmulator.SDK.Seeding.Interfaces;
using Cosmos.Phantom.InMemoryEmulator.SDK.Seeding.Services;
using Cosmos.Phantom.InMemoryEmulator.SDK.Exceptions;
using Cosmos.Phantom.InMemoryEmulator.SDK.Configuration;
using Cosmos.Phantom.InMemoryEmulator.SDK.ChaosEngineering;
using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;

using CosmosDB.InMemoryEmulator;

namespace Cosmos.Phantom.InMemoryEmulator.SDK;

public static class PhantomCosmosExtensions
{
    /// <summary>
    /// Registra apenas o CosmosClient emulado e os serviços de Seeding.
    /// Em produção, a API consumidora deve registrar seu próprio CosmosClient.
    /// </summary>
    public static IServiceCollection AddCosmosPhantomEmulator(
        this IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        // Fail-Fast Silencioso: Se não for dev ou a flag estiver desligada, ignora o SDK.
        if (!environment.IsDevelopment() || !configuration.GetValue<bool>("UseCosmosDbEmulator"))
        {
            return services;
        }

        // 1. Resolve configurações: Lê o que o usuário forneceu, ou faz fallback pro arquivo embutido
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
            emulatorConfig.Chaos ??= fallbackConfig.Chaos;
        }

        if (emulatorConfig == null) return services;

        // Configura as Options injetáveis para que outros serviços do SDK possam acessar
        services.Configure<CosmosDbEmulatorConfig>(options =>
        {
            options.DatabaseName = emulatorConfig.DatabaseName;
            options.Containers = emulatorConfig.Containers;
            options.Chaos = emulatorConfig.Chaos;
        });

        // Valida as anotações do modelo final
        services.AddOptions<CosmosDbEmulatorConfig>()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.UseInMemoryCosmosDB(options =>
        {
            options.DatabaseName = emulatorConfig.DatabaseName;
            if (emulatorConfig.Containers != null)
            {
                foreach (var container in emulatorConfig.Containers)
                {
                    options.AddContainer(container.Name, container.PartitionKeyPath);
                }
            }

            options.OnHandlerCreated = (name, handler) =>
            {
                ChaosEngineeringConfigurator.ConfigureFaultInjector(handler, configuration);
            };
        });

        // Registra os serviços para a carga de dados (Seeding)
        services.AddTransient<ISeedFileReader, SeedFileReader>();
        services.AddTransient<ICosmosDbManager, CosmosDbManager>();
        services.AddTransient<ICosmosDbSeederService, CosmosDbSeederService>();

        return services;
    }

    private static CosmosDbEmulatorConfig? LoadEmbeddedConfig()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Cosmos.Phantom.InMemoryEmulator.SDK.Resources.Cosmos.Emulator.Config.json";
            
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
