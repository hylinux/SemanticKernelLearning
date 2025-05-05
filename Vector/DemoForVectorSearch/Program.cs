using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Embeddings;
using Qdrant.Client;

var vectorStore = new QdrantVectorStore(
    new QdrantClient("localhost")
);

var collection = vectorStore.GetCollection<ulong, Hotel>("skhotels4");

async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text)
{
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    AzureOpenAITextEmbeddingGenerationService textEmbeddingGenerationService = new(
        deploymentName: "text-embedding-3-large",
        endpoint: "",
        apiKey: "",
        modelId: "text-embedding-3-large",
        dimensions: 1536
    );
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

    var embedding = await textEmbeddingGenerationService.GenerateEmbeddingAsync(text);
    return embedding;

}


await collection.CreateCollectionIfNotExistsAsync();

ReadOnlyMemory<float> searchVector = await GenerateEmbeddingAsync("我在寻找一个适合家庭的酒店，价格在100到200美元之间。");

var searchResult = collection.SearchEmbeddingAsync(searchVector, top:5);

await foreach ( var record in searchResult)
{
    Console.WriteLine($"酒店的名字: {record.Record.HotelName}");
    Console.WriteLine("酒店的描述: " + record.Record.Description);
    Console.WriteLine("酒店的评分: " + record.Score);
    Console.WriteLine("酒店的标签: " + string.Join(", ", record.Record.Tags ?? Array.Empty<string>()));
    
}
