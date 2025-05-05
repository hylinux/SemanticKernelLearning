using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Embeddings;
using Qdrant.Client;

var vectorStore = new QdrantVectorStore(
    new QdrantClient("localhost")
);

var collection = vectorStore.GetCollection<ulong, Hotel>("skhotels");

async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text)
{
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    AzureOpenAITextEmbeddingGenerationService textEmbeddingGenerationService = new (
        deploymentName: "text-embedding-3-large",
        endpoint: "https://<your-endpoint>.openai.azure.com/",
        apiKey: "<your-api-key>",
        modelId: "text-embedding-3-large",
        dimensions: 1536
    );
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

    var embedding = await textEmbeddingGenerationService.GenerateEmbeddingAsync(text);
    return embedding;

}


await collection.CreateCollectionIfNotExistsAsync();

string descrptionText = "A place where everyone can be happy";

ulong hotelId = 1;

await collection.UpsertAsync(new Hotel {
    HotelId = hotelId,
    HotelName = "Hotel California",
    Description = descrptionText,
    DescriptionEmbedding = await GenerateEmbeddingAsync(descrptionText),
    Tags = new[] { "luxury", "spa", "pool" }
});

for(int i = 2; i <= 100; i++)
{
    await collection.UpsertAsync( new Hotel {
        HotelId = (ulong)i,
        HotelName = "Hotel California" + i.ToString(),
        Description = descrptionText + i.ToString(),
        DescriptionEmbedding = await GenerateEmbeddingAsync(descrptionText + i.ToString()),
        Tags = ["luxury", "spa", "pool"]
    });
}


Hotel? retrievedHotel = await collection.GetAsync(hotelId);
Console.WriteLine("Hotel name: " + retrievedHotel!.HotelName);
Console.WriteLine("Hotel description: " + retrievedHotel.Description);
if ( retrievedHotel.Tags != null )
{
    Console.WriteLine("Hotel tags: " + string.Join(", ", retrievedHotel!.Tags));
}



//做一次搜索
ReadOnlyMemory<float> searchVector = await GenerateEmbeddingAsync("I'm looking for a hotel where customer happiness is the priority.");

var searchResult = collection.SearchEmbeddingAsync(searchVector, top:5);

await foreach ( var record in searchResult )
{
    Console.WriteLine("Found hotel descrption: " + record.Record.Description);
    Console.WriteLine("Found hotel name: " + record.Record.HotelName);
    Console.WriteLine("Found record score: " + record.Score);

}

