using System;

namespace Cosmos.Phantom.InMemoryEmulator.SDK.Exceptions;

/// <summary>
/// Exceção lançada quando as configurações do Cosmos DB Emulator no appsettings.json estão ausentes ou inválidas.
/// </summary>
public class CosmosDbEmulatorConfigurationException : Exception
{
    public CosmosDbEmulatorConfigurationException(string message) 
        : base(message)
    {
    }

    public CosmosDbEmulatorConfigurationException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
