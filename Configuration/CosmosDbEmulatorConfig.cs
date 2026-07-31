using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cosmos.Phantom.SDK.Configuration;

public class CosmosDbEmulatorConfig
{
    [Required(ErrorMessage = "A propriedade 'DatabaseName' é obrigatória na configuração do CosmosDbEmulator.")]
    public required string DatabaseName { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "A lista de 'Containers' está vazia ou ausente. Ao menos um container deve ser configurado.")]
    public List<ContainerConfig> Containers { get; set; } = [];
}

public class ContainerConfig
{
    [Required(ErrorMessage = "Todos os containers configurados devem possuir 'Name'.")]
    public required string Name { get; set; }

    [Required(ErrorMessage = "Todos os containers configurados devem possuir 'PartitionKeyPath'.")]
    public string PartitionKeyPath { get; set; } = "/id";
    
    public required EmulatorIndexingPolicy IndexingPolicy { get; set; }
}

public class EmulatorIndexingPolicy
{
    public IndexingMode IndexingMode { get; set; } = IndexingMode.Consistent;
    public bool Automatic { get; set; } = true;
    public List<string> IncludedPaths { get; set; } = [];
    public List<string> ExcludedPaths { get; set; } = [];
    public List<List<EmulatorCompositePath>> CompositeIndexes { get; set; } = [];
}

public class EmulatorCompositePath
{
    public required string Path { get; set; }
    public string Order { get; set; } = "ascending";
}
