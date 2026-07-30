using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Cosmos.Phantom.InMemoryEmulator.SDK.ChaosEngineering;

namespace Cosmos.Phantom.InMemoryEmulator.SDK.Configuration;

public class CosmosDbEmulatorConfig
{
    [Required(ErrorMessage = "A propriedade 'DatabaseName' é obrigatória na configuração do CosmosDbEmulator.")]
    public string DatabaseName { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "A lista de 'Containers' está vazia ou ausente. Ao menos um container deve ser configurado.")]
    public List<ContainerConfig> Containers { get; set; } = new();

    public ChaosConfig? Chaos { get; set; }
}

public class ContainerConfig
{
    [Required(ErrorMessage = "Todos os containers configurados devem possuir 'Name'.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Todos os containers configurados devem possuir 'PartitionKeyPath'.")]
    public string PartitionKeyPath { get; set; } = "/id";
    
    public EmulatorIndexingPolicy IndexingPolicy { get; set; }
}

public class EmulatorIndexingPolicy
{
    public IndexingMode Mode { get; set; } = IndexingMode.Consistent;
    public bool Automatic { get; set; } = true;
    public List<string> IncludedPaths { get; set; } = new();
    public List<string> ExcludedPaths { get; set; } = new();
    public List<List<EmulatorCompositePath>> CompositeIndexes { get; set; } = new();
}

public class EmulatorCompositePath
{
    public string Path { get; set; }
    public string Order { get; set; } = "ascending";
}
