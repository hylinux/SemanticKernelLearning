using System.Globalization;
using Azure;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using VectorStoreRag;
using VectorStoreRag.Options;


HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

//添加本地的secrets的配置
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddUserSecrets<Program>();
builder.Configuration.AddEnvironmentVariables();
builder.Services.Configure<RagConfig>(
    builder.Configuration.GetSection(RagConfig.ConfigSectionName)
);


//载入所有的配置
var appConfig = new ApplicationConfig(builder.Configuration);

//添加一个Cancellation Token到应用服务中，允许通过它进行gracefull关闭
CancellationTokenSource appShutdownCancellationTokenSource = new();
CancellationToken appShutdownCancellationToken = appShutdownCancellationTokenSource.Token;

builder.Services.AddKeyedSingleton("AppShutdown", appShutdownCancellationTokenSource);

//将SK Kernel注册到依赖注入容器中，并且添加Chat Completion服务以及Text Embedding 生成服务
//var kernelBuilder = builder.Services.AddKernel();

switch(appConfig.RagConfig.AIChatService)
{
    case "AzureOpenAI":
        builder.Services.AddSingleton<IChatCompletionService>(
            new AzureOpenAIChatCompletionService(
                deploymentName:appConfig.AzureOpenAIConfig.ChatDeploymentName,
                apiKey: appConfig.AzureOpenAIConfig.ApiKey,
                endpoint: appConfig.AzureOpenAIConfig.Endpoint,
                modelId: appConfig.AzureOpenAIConfig.ChatDeploymentName
            )
        );

        break;
    case "OpenAI":
        builder.Services.AddOpenAIChatCompletion(
            appConfig.OpenAIConfig.ModelId,
            appConfig.OpenAIConfig.ApiKey,
            appConfig.OpenAIConfig.OrgId
        );
        break;
    //leave here for DeepSeek
    default:
        throw new NotSupportedException(
            $"AI Chat Service Type '{appConfig.RagConfig.AIChatService}' is not supported."
        );
}




//添加text embedding生成服务
switch (appConfig.RagConfig.AIEmbeddingService)
{
    case "AzureOpenAIEmbeddings":
        builder.Services.AddSingleton<ITextEmbeddingGenerationService>(
            new AzureOpenAITextEmbeddingGenerationService(
                appConfig.AzureOpenAIEmbeddingsConfig.DeploymentName,
                appConfig.AzureOpenAIEmbeddingsConfig.Endpoint,
                appConfig.AzureOpenAIEmbeddingsConfig.ApiKey,
                appConfig.AzureOpenAIEmbeddingsConfig.DeploymentName
            )
        );
        break;
    case "OpenAIEmbeddings":
        builder.Services.AddOpenAITextEmbeddingGeneration(
            appConfig.OpenAIEmbeddingsConfig.ModelId,
            appConfig.OpenAIEmbeddingsConfig.ApiKey,
            appConfig.OpenAIEmbeddingsConfig.OrgId);
        break;
    default:
        throw new NotSupportedException($"AI Embedding Service type '{appConfig.RagConfig.AIEmbeddingService}' is not supported.");
        
}


//添加向量数据库
switch (appConfig.RagConfig.VectorStoreType)
{
    case "AzureAISearch":
        builder.Services.AddAzureAISearchVectorStoreRecordCollection<TextSnippet<string>>(
            appConfig.RagConfig.CollectionName,
            new Uri(appConfig.AzureAISearchConfig.Endpoint),
            new AzureKeyCredential(appConfig.AzureAISearchConfig.ApiKey));
        break;
    case "AzureCosmosDBMongoDB":
        builder.Services.AddAzureCosmosDBMongoDBVectorStoreRecordCollection<TextSnippet<string>>(
            appConfig.RagConfig.CollectionName,
            appConfig.AzureCosmosDBMongoDBConfig.ConnectionString,
            appConfig.AzureCosmosDBMongoDBConfig.DatabaseName);
        break;
    case "AzureCosmosDBNoSQL":
        builder.Services.AddAzureCosmosDBNoSQLVectorStoreRecordCollection<TextSnippet<string>>(
            appConfig.RagConfig.CollectionName,
            appConfig.AzureCosmosDBNoSQLConfig.ConnectionString,
            appConfig.AzureCosmosDBNoSQLConfig.DatabaseName);
        break;
    case "InMemory":
        builder.Services.AddInMemoryVectorStoreRecordCollection<string, TextSnippet<string>>(
            appConfig.RagConfig.CollectionName);
        break;
    case "Qdrant":
        builder.Services.AddQdrantVectorStoreRecordCollection<Guid, TextSnippet<Guid>>(
            appConfig.RagConfig.CollectionName,
            appConfig.QdrantConfig.Host,
            appConfig.QdrantConfig.Port,
            appConfig.QdrantConfig.Https,
            appConfig.QdrantConfig.ApiKey);
        break;
    case "Redis":
        builder.Services.AddRedisJsonVectorStoreRecordCollection<TextSnippet<string>>(
            appConfig.RagConfig.CollectionName,
            appConfig.RedisConfig.ConnectionConfiguration);
        break;
    case "Weaviate":
        builder.Services.AddWeaviateVectorStoreRecordCollection<TextSnippet<Guid>>(
            // Weaviate collection names must start with an upper case letter.
            char.ToUpper(appConfig.RagConfig.CollectionName[0], CultureInfo.InvariantCulture) + appConfig.RagConfig.CollectionName.Substring(1),
            null,
            new() { Endpoint = new Uri(appConfig.WeaviateConfig.Endpoint) });
        break;
    default:
        throw new NotSupportedException($"Vector store type '{appConfig.RagConfig.VectorStoreType}' is not supported.");
}



// Register all the other required services.
switch (appConfig.RagConfig.VectorStoreType)
{
    case "AzureAISearch":
    case "AzureCosmosDBMongoDB":
    case "AzureCosmosDBNoSQL":
    case "InMemory":
    case "Redis":
        RegisterServices<string>(builder, appConfig);
        break;
    case "Qdrant":
    case "Weaviate":
        RegisterServices<Guid>(builder, appConfig);
        break;
    default:
        throw new NotSupportedException($"Vector store type '{appConfig.RagConfig.VectorStoreType}' is not supported.");
}

builder.Services.AddTransient((serviceProvider) => {
    return new Kernel(serviceProvider);
});


// Build and run the host.
using IHost host = builder.Build();
await host.RunAsync(appShutdownCancellationToken).ConfigureAwait(false);


static void RegisterServices<TKey>(HostApplicationBuilder builder, ApplicationConfig vectorStoreRagConfig)
    where TKey : notnull
{
    // Add a text search implementation that uses the registered vector store record collection for search.
    builder.Services.AddVectorStoreTextSearch<TextSnippet<TKey>>(
        new TextSearchStringMapper((result) => (result as TextSnippet<TKey>)!.Text!),
        new TextSearchResultMapper((result) =>
        {
            // Create a mapping from the Vector Store data type to the data type returned by the Text Search.
            // This text search will ultimately be used in a plugin and this TextSearchResult will be returned to the prompt template
            // when the plugin is invoked from the prompt template.
            var castResult = result as TextSnippet<TKey>;
            return new TextSearchResult(value: castResult!.Text!) { Name = castResult.ReferenceDescription, Link = castResult.ReferenceLink };
        }));

    // Add the key generator and data loader to the dependency injection container.
    builder.Services.AddSingleton<UniqueKeyGenerator<Guid>>(
        new UniqueKeyGenerator<Guid>(() => Guid.NewGuid()));
    builder.Services.AddSingleton<UniqueKeyGenerator<string>>(new UniqueKeyGenerator<string>(() => Guid.NewGuid().ToString()));
    builder.Services.AddSingleton<IDataLoader, DataLoader<TKey>>();

    // Add the main service for this application.
    builder.Services.AddHostedService<RAGChatService<TKey>>();
}







