using System.Net;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;

namespace VectorStoreRag;


internal class DataLoader<TKey>(
    UniqueKeyGenerator<TKey> uniqueKeyGenerator,
    IVectorStoreRecordCollection<TKey, TextSnippet<TKey>> vectorStoreRecordCollection,
    ITextEmbeddingGenerationService textEmbeddingGenerationService,
    IChatCompletionService chatCompletionService) : IDataLoader where TKey : notnull
{
    public async Task LoadPdf(string pdfPath, int batchSize, int betweenBatchDelayInMs, CancellationToken cancellationToken)
    {

        await vectorStoreRecordCollection.CreateCollectionIfNotExistsAsync(cancellationToken).ConfigureAwait(false);

        var sections = LoadTextAndImages(pdfPath, cancellationToken);
        var batches =  sections.Chunk(batchSize);





    }


    //将PDF文件中的文本和图片分别读取出来。
    private static IEnumerable<RawContent> 




    //使用emebedding模型生成文本的embeeding
    private static async Task<ReadOnlyMemory<float>> GenerateEmbeddingsWithRetryAsync(
        ITextEmbeddingGenerationService textEmbeddingGenerationService,
        string text,
        CancellationToken cancellationToken)
    {
        var tries = 0;

        while ( true )
        {
            try 
            {

                return await textEmbeddingGenerationService.GenerateEmbeddingAsync(text,
                    cancellationToken: cancellationToken).ConfigureAwait(false);
            } catch ( HttpOperationException ex ) when ( ex.StatusCode == HttpStatusCode.TooManyRequests )
            {
                tries++;

                if ( tries < 3 )
                {
                    Console.WriteLine($"Failed to generate embedding. Error: {ex}");
                    Console.WriteLine($"Retrying embedding generation...");
                    await Task.Delay(10_000, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    throw;
                }
            }
        }
    }
     




    //让大模型识别上传的图片
    private static async Task<string> ConvertImageToTextWithRetryAsync(
        IChatCompletionService chatCompletionService,
        ReadOnlyMemory<byte> imageBytes,
        CancellationToken cancellationToken)
    {
        var tries = 0;

        while ( true) 
        {
            try 
            {
            
                var chatHistory = new ChatHistory();
                chatHistory.AddUserMessage([
                    new TextContent("这张图片里有什么内容?"),
                    new ImageContent(imageBytes, "image/png"),
                ]);

                var result = await chatCompletionService.GetChatMessageContentsAsync(chatHistory,
                    cancellationToken: cancellationToken).ConfigureAwait(false);
                return string.Join("\n", result.Select( x => x.Content  ));
            } 
            catch ( HttpOperationException ex) when ( ex.StatusCode == HttpStatusCode.TooManyRequests)
            {
                tries++;

                if ( tries < 3 )
                {
                    Console.WriteLine($"Failed to generate text from image. Error: {ex}");
                    Console.WriteLine($"Retrying text to image conversation...");
                    await Task.Delay(10_000, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    throw;
                }
            }
        }
    }



    //为从一个PDF文件中读出的内容，创建一个基本的模型。
    private sealed class RawContent
    {
        public string? Text {get; set;}

        public ReadOnlyMemory<byte>? Image {get; set;}

        public int PageNumber { get; set;}

    }


}


