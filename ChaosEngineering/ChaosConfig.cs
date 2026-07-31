namespace Cosmos.Phantom.SDK.ChaosEngineering;

public class ChaosConfig
{
    public bool EnableRandomThrottling { get; set; } = false;
    public double ThrottlingProbability { get; set; } = 0.2;
    
    public bool SimulateHighLatency { get; set; } = false;
    public int MinLatencyMs { get; set; } = 500;
    public int MaxLatencyMs { get; set; } = 2000;
    
    public bool Simulate429_TooManyRequests { get; set; }
    public bool Simulate503_ServiceUnavailable { get; set; }
    public bool Simulate408_RequestTimeout { get; set; }
    public bool Simulate403_Forbidden { get; set; }
    public bool Simulate401_Unauthorized { get; set; }
    public bool Simulate409_Conflict { get; set; }
    public bool Simulate413_EntityTooLarge { get; set; }
    public bool Simulate412_PreconditionFailed { get; set; }
    public bool Simulate400_BadRequest { get; set; }
}
