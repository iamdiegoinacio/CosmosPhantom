namespace Cosmos.Phantom.InMemoryEmulator.SDK.ChaosEngineering;

public class ChaosConfig
{
    public bool EnableThrottlingMode { get; set; } = false;
    public double ThrottlingRate { get; set; } = 0.2;
    
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
