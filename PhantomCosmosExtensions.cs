using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Cosmos.Phantom.SDK.Seeding;
using Cosmos.Phantom.SDK.Exceptions;
using Cosmos.Phantom.SDK.Configuration;
using Cosmos.Phantom.SDK.ChaosEngineering;
using System;

using CosmosDB.InMemoryEmulator;

namespace Cosmos.Phantom.SDK;

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
        if (!environment.IsDevelopment() || !configuration.GetValue<bool>("UseCosmosPhantom"))
        {
            return services;
        }

        // 1. Resolve configurações (Usuário vs Embutido)
        var emulatorConfig = CosmosEmulatorConfigResolver.Resolve(configuration);
        var chaosConfig = CosmosChaosConfigResolver.Resolve(configuration);
        if (emulatorConfig == null) return services;

        // 2. Configura e valida as Options
        services.Configure<CosmosDbEmulatorConfig>(options =>
        {
            options.DatabaseName = emulatorConfig.DatabaseName;
            options.Containers = emulatorConfig.Containers;
        });

        services.AddOptions<CosmosDbEmulatorConfig>()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // 3. Inicializa o emulador e o injetor de caos
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
                ChaosEngineeringConfigurator.ConfigureFaultInjector(handler, chaosConfig);
            };
        });

        // 4. Registra serviços complementares
        services.AddPhantomSeedingServices();

        return services;
    }
}
