using System.ComponentModel.DataAnnotations;

namespace VectorStoreRag.Options;

internal sealed class RedisConfig
{
    public const string ConfigSectionName = "Redis";

    [Required]
    public string ConnectionConfiguration {get; set;} = string.Empty;

}