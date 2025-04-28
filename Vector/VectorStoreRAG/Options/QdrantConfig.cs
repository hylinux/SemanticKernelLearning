using System.ComponentModel.DataAnnotations;

namespace VectorStoreRag.Options;

internal sealed class QdrantConfig
{
    public const string ConfigSectionName = "Qdrant";

    [Required]
    public string Host {get; set;} = string.Empty;

    public int Port {get; set;} = 6333;

    public bool Https {get; set;} = false;

    public string ApiKey {get; set;} = string.Empty;
}