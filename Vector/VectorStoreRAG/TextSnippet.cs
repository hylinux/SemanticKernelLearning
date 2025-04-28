using Microsoft.Extensions.VectorData;

namespace VectorStoreRag;

internal sealed class TextSnippet<TKey>
{
    [VectorStoreRecordKey]
    public required TKey Key { get; set; }

    [VectorStoreRecordData]
    public string? Text { get; set; }

    [VectorStoreRecordData]
    public string? ReferenceDescription { get; set; }

    [VectorStoreRecordData]
    public string? ReferenceLink { get; set; }

    [VectorStoreRecordVector(1536)]
    public ReadOnlyMemory<float> TextEmbedding { get; set; }
}


