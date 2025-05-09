using System.ComponentModel.DataAnnotations;

namespace VectorStoreRag.Options;

internal sealed class AzureOpenAIEmbeddingsConfig
{
    public const string ConfigSectionName = "AzureOpenAIEmbeddings";

    [Required]
    public string DeploymentName {get; set; } = string.Empty;

    [Required]
    public string Endpoint {get; set; } = string.Empty;

    [Required]
    public string ApiKey {get; set;} = string.Empty;


}