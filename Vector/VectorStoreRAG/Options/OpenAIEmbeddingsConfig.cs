using System.ComponentModel.DataAnnotations;

namespace VectorStoreRag.Options;

internal sealed class OpenAIEmbeddingsConfig
{
    public const string ConfigSectionName = "OpenAIEmbeddings";

    [Required]
    public string ModelId { get; set; } = string.Empty;

    [Required]
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    public string? OrgId { get; set; } = string.Empty;
    

}