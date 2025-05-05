using System.ComponentModel.DataAnnotations;

namespace VectorStoreRag.Options;

internal sealed class AzureOpenAIConfig
{
    public const string ConfigSectionName = "AzureOpenAI";

    [Required]
    public string ChatDeploymentName {get; set;} = string.Empty;

    [Required]
    public string Endpoint {get; set; } = string.Empty;

    [Required]
    public string ApiKey {get; set;} = string.Empty;



}