using Microsoft.Extensions.DependencyInjection;
using Cosmos.Phantom.SDK.Seeding.Interfaces;
using Cosmos.Phantom.SDK.Seeding.Services;

namespace Cosmos.Phantom.SDK.Seeding;

internal static class SeedingServiceCollectionExtensions
{
    /// <summary>
    /// Registra os serviÃ§os responsÃ¡veis pela carga de dados (Seeding) no Cosmos DB.
    /// </summary>
    public static IServiceCollection AddPhantomSeedingServices(this IServiceCollection services)
    {
        services.AddTransient<ISeedFileReader, SeedFileReader>();
        services.AddTransient<ICosmosDbManager, CosmosDbManager>();
        services.AddTransient<ICosmosDbBulkInserter, CosmosDbBulkInserter>();
        services.AddTransient<ICosmosDbSeederService, CosmosDbSeederService>();

        return services;
    }
}
