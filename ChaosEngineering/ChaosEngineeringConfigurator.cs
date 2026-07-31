using CosmosDB.InMemoryEmulator;
using System;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.Cosmos;

namespace Cosmos.Phantom.SDK.ChaosEngineering;

public static class ChaosEngineeringConfigurator
{
    public static bool IsBypassed { get; set; } = false;

    public static void ConfigureFaultInjector(FakeCosmosHandler handler, ChaosConfig? chaosConfig)
    {
        if (chaosConfig == null) return;

        var random = new Random();
        handler.FaultInjector = req =>
        {
            if (IsBypassed) return null;

            // Não injetar falhas em requisições de metadados do Cosmos SDK (apenas em documentos)
            if (req.RequestUri != null && !req.RequestUri.AbsolutePath.Contains("/docs", StringComparison.OrdinalIgnoreCase))
                return null; 

            // Simulação de Alta Latência (Ping / Delay)
            if (chaosConfig.SimulateHighLatency)
            {
                int delay = random.Next(chaosConfig.MinLatencyMs, chaosConfig.MaxLatencyMs);
                System.Threading.Thread.Sleep(delay);
            }

            // Simulação de Throttling Randômico
            if (chaosConfig.EnableRandomThrottling && random.NextDouble() > chaosConfig.ThrottlingProbability)
                return null; 

            return ObterFalhaSimulada(chaosConfig);
        };
    }

    private static HttpResponseMessage? ObterFalhaSimulada(ChaosConfig chaos)
    {
        // O Cosmos SDK traduz automaticamente respostas HTTP para CosmosException. 
        return true switch
        {
            _ when chaos.Simulate429_TooManyRequests => CriarRespostaCosmos(429, 3200),
            _ when chaos.Simulate503_ServiceUnavailable => new HttpResponseMessage((HttpStatusCode)503),
            _ when chaos.Simulate408_RequestTimeout => new HttpResponseMessage((HttpStatusCode)408),
            _ when chaos.Simulate403_Forbidden => new HttpResponseMessage((HttpStatusCode)403),
            _ when chaos.Simulate401_Unauthorized => new HttpResponseMessage((HttpStatusCode)401),
            _ when chaos.Simulate409_Conflict => new HttpResponseMessage((HttpStatusCode)409),
            _ when chaos.Simulate413_EntityTooLarge => new HttpResponseMessage((HttpStatusCode)413),
            _ when chaos.Simulate412_PreconditionFailed => new HttpResponseMessage((HttpStatusCode)412),
            _ when chaos.Simulate400_BadRequest => new HttpResponseMessage((HttpStatusCode)400),
            _ => null // Sem falha configurada
        };
    }

    private static HttpResponseMessage CriarRespostaCosmos(int statusCode, int subStatus)
    {
        var response = new HttpResponseMessage((HttpStatusCode)statusCode);
        response.Headers.Add("x-ms-substatus", subStatus.ToString());
        response.Headers.Add("x-ms-activity-id", Guid.NewGuid().ToString());
        return response;
    }
}
