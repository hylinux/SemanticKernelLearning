using System.ComponentModel.DataAnnotations;

namespace VectorStoreRag.Options;

internal sealed class WeaviateConfig
{
    public const string ConfigSectionName = "Weaviate";

    [Required]
    public string Endpoint {get; set;} = string.Empty;
    
}