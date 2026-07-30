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

        // Delega validação ao ASP.NET Core
        services.AddOptions<CosmosDbEmulatorConfig>()
            .Bind(configuration.GetSection("CosmosDbEmulator"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var emulatorConfig = configuration.GetSection("CosmosDbEmulator").Get<CosmosDbEmulatorConfig>();
        if (emulatorConfig == null) return services;

        services.UseInMemoryCosmosDB(options =>
        {
            options.DatabaseName = emulatorConfig.DatabaseName;
            foreach (var container in emulatorConfig.Containers)
            {
                options.AddContainer(container.Name, container.PartitionKeyPath);
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
}
