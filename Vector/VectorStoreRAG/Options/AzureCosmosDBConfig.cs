using System.ComponentModel.DataAnnotations;

namespace VectorStoreRag.Options;

internal sealed class AzureCosmosDBConfig
{
    public const string MongoDBConfigSectionName = "AzureCosmosDBMongoDB";
    public const string NoSQLConfigSectionName = "AzureCosmosDBNoSQL"; 

    [Required]
    public string ConnectionString {get; set;} = string.Empty;

    [Required]
    public string DatabaseName {get; set;} = string.Empty;

}