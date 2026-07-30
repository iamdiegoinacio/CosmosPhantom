using CosmosDB.InMemoryEmulator;
using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos;

namespace Cosmos.Phantom.InMemoryEmulator.SDK.ChaosEngineering;

public static class ChaosEngineeringConfigurator
{
    public static bool IsBypassed { get; set; } = false;

    public static void ConfigureFaultInjector(FakeCosmosHandler handler, IConfiguration configuration)
    {
        var random = new Random();
        handler.FaultInjector = req =>
        {
            if (IsBypassed) return null;

            var chaos = configuration.GetSection("CosmosDbEmulator:Chaos").Get<ChaosConfig>();
            if (chaos == null) return null;

            // Não injetar falhas em requisiÃ§Ãµes de metadados do Cosmos SDK (apenas em documentos)
            if (req.RequestUri != null && !req.RequestUri.AbsolutePath.Contains("/docs", StringComparison.OrdinalIgnoreCase))
                return null; 

            // Simulação de Throttling Randômico
            if (chaos.EnableThrottlingMode && random.NextDouble() > chaos.ThrottlingRate)
                return null; 

            return ObterFalhaSimulada(chaos);
        };
    }

    private static HttpResponseMessage? ObterFalhaSimulada(ChaosConfig chaos)
    {
        // O Cosmos SDK traduz automaticamente respostas HTTP para CosmosException. 
        if (chaos.Simulate429_TooManyRequests) return CriarRespostaCosmos(429, 3200);
        if (chaos.Simulate503_ServiceUnavailable) return new HttpResponseMessage((HttpStatusCode)503);
        if (chaos.Simulate408_RequestTimeout) return new HttpResponseMessage((HttpStatusCode)408);
        if (chaos.Simulate403_Forbidden) return new HttpResponseMessage((HttpStatusCode)403);
        if (chaos.Simulate401_Unauthorized) return new HttpResponseMessage((HttpStatusCode)401);
        if (chaos.Simulate409_Conflict) return new HttpResponseMessage((HttpStatusCode)409);
        if (chaos.Simulate413_EntityTooLarge) return new HttpResponseMessage((HttpStatusCode)413);
        if (chaos.Simulate412_PreconditionFailed) return new HttpResponseMessage((HttpStatusCode)412);
        if (chaos.Simulate400_BadRequest) return new HttpResponseMessage((HttpStatusCode)400);

        return null; // Sem falha configurada
    }

    private static HttpResponseMessage CriarRespostaCosmos(int statusCode, int subStatus)
    {
        var response = new HttpResponseMessage((HttpStatusCode)statusCode);
        response.Headers.Add("x-ms-substatus", subStatus.ToString());
        response.Headers.Add("x-ms-activity-id", Guid.NewGuid().ToString());
        return response;
    }
}
