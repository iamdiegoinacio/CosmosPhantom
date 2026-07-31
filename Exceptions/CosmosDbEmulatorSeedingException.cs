using System;

namespace Cosmos.Phantom.SDK.Exceptions;

/// <summary>
/// Exceção lançada quando a leitura, parser ou inserção do arquivo de seed (JSON) falha de maneira irrecuperável.
/// </summary>
public class CosmosDbEmulatorSeedingException : Exception
{
    public CosmosDbEmulatorSeedingException(string message) 
        : base(message)
    {
    }

    public CosmosDbEmulatorSeedingException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
