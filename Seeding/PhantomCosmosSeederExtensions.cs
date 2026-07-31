using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Cosmos.Phantom.SDK.Configuration;
using Cosmos.Phantom.SDK.ChaosEngineering;
using Cosmos.Phantom.SDK.Seeding.Interfaces;

namespace Cosmos.Phantom.SDK.Seeding;

public static class PhantomCosmosSeederExtensions
{
    /// <summary>
    /// Ler as configurações para criar o banco, containers, políticas e insere os dados JSON correspondentes.
    /// Utiliza serviços especializados injetados pelo DI para garantir o princípio da responsabilidade única.
    /// </summary>
    public static async Task UseCosmosPhantomSeederAsync(
        this IServiceProvider services,
        CancellationToken ct = default)
    {
        var environment = services.GetRequiredService<IHostEnvironment>();
        var config = services.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
        
        // Fail-Fast Silencioso
        if (!environment.IsDevelopment() || !config.GetValue<bool>("UseCosmosDbEmulator"))
        {
            return;
        }

        var emulatorConfig = config.GetSection("CosmosDbEmulator").Get<CosmosDbEmulatorConfig>();
        if (emulatorConfig == null) return;

        var seederService = services.GetRequiredService<ICosmosDbSeederService>();
        var seedsFolderPath = System.IO.Path.Combine(environment.ContentRootPath, "Seeds");
        
        try
        {
            ChaosEngineeringConfigurator.IsBypassed = true;
            await seederService.SeedAsync(seedsFolderPath, ct);
        }
        finally
        {
            ChaosEngineeringConfigurator.IsBypassed = false;
        }
    }
}
