using System.ComponentModel.DataAnnotations;

namespace VectorStoreRag.Options;

internal sealed class OpenAIConfig
{
    public const string ConfigSectionName = "OpenAI";

    [Required]
    public string ModelId { get; set; } = string.Empty;

    [Required]
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    public string? OrgId { get; set; } = string.Empty;


}