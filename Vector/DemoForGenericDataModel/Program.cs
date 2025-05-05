using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Embeddings;
using Qdrant.Client;
using Microsoft.Extensions.VectorData;

//定义一个VectorStoreRecordDefinition
// 这个定义包含了我们要存储的向量数据的结构和属性
var hotelDefinition = new VectorStoreRecordDefinition
{
    Properties = new List<VectorStoreRecordProperty>
    {
        new VectorStoreRecordKeyProperty("HotelId", typeof(ulong)),
        new VectorStoreRecordDataProperty("HotelName", typeof(string)) {
            IsIndexed = true,
        },
        new VectorStoreRecordDataProperty("Description", typeof(string) ){
            IsFullTextIndexed = true,
        },

        new VectorStoreRecordVectorProperty("DescriptionEmbedding", typeof(ReadOnlyMemory<float>?), dimensions:1536 ){
            DistanceFunction = DistanceFunction.CosineSimilarity,
            IndexKind = IndexKind.Hnsw
        }
    }
};

var vectorStore = new QdrantVectorStore(new QdrantClient("localhost"));

var collection = vectorStore.GetCollection<object, Dictionary<string, object?>>("glossary", hotelDefinition);



async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text)
{
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    AzureOpenAITextEmbeddingGenerationService textEmbeddingGenerationService = new(
        deploymentName: "text-embedding-3-large",
        endpoint: "https://your-endpoint.openai.azure.com/",
        apiKey: "your-api-key",
        modelId: "text-embedding-3-large",
        dimensions: 1536
    );
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

    var embedding = await textEmbeddingGenerationService.GenerateEmbeddingAsync(text);
    return embedding;

}

await collection.CreateCollectionIfNotExistsAsync();




string descrptionText = "一个令人开心的地方，拥有舒适的床和美味的食物。";

ulong hotelId = 1;

await collection.UpsertAsync(new Hotel
{
    HotelId = hotelId,
    HotelName = "加利福利亚大酒店",
    Description = descrptionText,
    DescriptionEmbedding = await GenerateEmbeddingAsync(descrptionText),
    Tags = new[] { "奢华", "spa", "奢华" }
});

for (int i = 2; i <= 100; i++)
{
    await collection.UpsertAsync(new Hotel
    {
        HotelId = (ulong)i,
        HotelName = "加利福利亚大酒店" + i.ToString(),
        Description = descrptionText + i.ToString(),
        DescriptionEmbedding = await GenerateEmbeddingAsync(descrptionText + i.ToString()),
        Tags = ["奢华", "spa", "奢华"]
    });
}


Hotel? retrievedHotel = await collection.GetAsync(83);
Console.WriteLine("Hotel name: " + retrievedHotel!.HotelName);
Console.WriteLine("Hotel description: " + retrievedHotel.Description);
if (retrievedHotel.Tags != null)
{
    Console.WriteLine("Hotel tags: " + string.Join(", ", retrievedHotel!.Tags));
}



//做一次搜索
ReadOnlyMemory<float> searchVector = await GenerateEmbeddingAsync("我在寻找一个舒适的酒店，拥有美味的食物和舒适的床。");

var searchResult = collection.SearchEmbeddingAsync(searchVector, top: 5);

await foreach (var record in searchResult)
{
    Console.WriteLine("Found hotel descrption: " + record.Record.Description);
    Console.WriteLine("Found hotel name: " + record.Record.HotelName);
    Console.WriteLine("Found record score: " + record.Score);

}

