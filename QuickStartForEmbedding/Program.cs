using System.Text;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Embeddings;


Parameters parameters = null!;

ParserResult<Parameters> result = Parser.Default.ParseArguments<Parameters>(args)
    .WithParsed<Parameters>(r => parameters = r)
    .WithNotParsed(errors =>
    {
        Environment.Exit(1);
    });


//取得大模型的默认配置
var modelId = parameters.ModelId ?? "gpt-3.5-turbo";
var apiKey  = parameters.ApiKey;
var endpoint = parameters.Endpoint;
var logLevel = parameters.LogLevel ?? "Information";
var DeploymentName = parameters.DeploymentName ?? "gpt-3.5-turbo";


//创建日志级别
var logLevelEnum = Enum.TryParse(logLevel, true, out LogLevel level) ? level : LogLevel.Information;

#pragma warning disable SKEXP0010
var builder = Kernel.CreateBuilder().AddAzureOpenAITextEmbeddingGeneration(
    deploymentName: DeploymentName,
    apiKey: apiKey!,
    endpoint: endpoint!,
    modelId: modelId,
    serviceId: "text-embedding",
    httpClient: new HttpClient(),
    dimensions: 1536
);
#pragma warning restore SKEXP0010

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.SetMinimumLevel(logLevelEnum);
});






var kernel = builder.Build();

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var textEmbedding = kernel.GetRequiredService<ITextEmbeddingGenerationService>();
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

var text = parameters.EmbeddingText ?? "Hello, World!";


ReadOnlyMemory<float> embedding = await textEmbedding.GenerateEmbeddingAsync(text);


Console.WriteLine($"Embedding for '{text}':\r\n { string.Join("\r\n", embedding.ToArray())}");
Console.WriteLine($"Embedding length: {embedding.Length}");




















