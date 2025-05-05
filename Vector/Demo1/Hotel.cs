using Microsoft.Extensions.VectorData;

internal class Hotel
{

    [VectorStoreRecordKey]
    public ulong HotelId {get; set; }

    [VectorStoreRecordData(IsIndexed = true)]
    public string HotelName {get; set; } = string.Empty;

    
    [VectorStoreRecordData(IsFullTextIndexed = true)]
    public string Description {get; set;} = string.Empty;

    [VectorStoreRecordVector(Dimensions: 1536, 
    DistanceFunction = DistanceFunction.CosineSimilarity,
    IndexKind = IndexKind.Hnsw
    )]
    public ReadOnlyMemory<float>? DescriptionEmbedding {get; set; }

    [VectorStoreRecordData(IsIndexed = true)]
    public string[]? Tags {get; set; }

}