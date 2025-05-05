using System.Net;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;

namespace VectorStoreRag;



/// <summary>
/// 数据加载类，用于将PDF文件中的文本加载到向量数据库中。
/// </summary>
/// <typeparam name="TKey">向量数据库的键类型</typeparam>
/// <param name="uniqueKeyGenerator">唯一键生成器</param>
/// <param name="vectorStoreRecordCollection">向量数据库记录集合</param>
/// <param name="textEmbeddingGenerationService">文本嵌入生成服务</param>
/// <param name="chatCompletionService">聊天完成服务</param>    
/// <remarks>该类实现了IDataLoader接口，提供了加载PDF文件的功能。</remarks>
internal class DataLoader<TKey>(
    UniqueKeyGenerator<TKey> uniqueKeyGenerator,
    IVectorStoreRecordCollection<TKey, TextSnippet<TKey>> vectorStoreRecordCollection,
    IChatCompletionService chatCompletionService,
    ITextEmbeddingGenerationService textEmbeddingGenerationService
    ) : IDataLoader where TKey : notnull
{


    public async Task LoadPdf(string pdfPath, int batchSize, int betweenBatchDelayInMs, CancellationToken cancellationToken)
    {

        await vectorStoreRecordCollection.CreateCollectionIfNotExistsAsync(cancellationToken).ConfigureAwait(false);

        var sections = LoadTextAndImages(pdfPath, cancellationToken);
        var batches =  sections.Chunk(batchSize);

        foreach ( var batch in batches )
        {
            var textContentTasks = batch.Select( async content => {

                if ( content.Text != null )
                {
                    return content;
                }

                var textFromImage = await ConvertImageToTextWithRetryAsync(
                    chatCompletionService,
                    content.Image!.Value,
                    cancellationToken).ConfigureAwait(false);

                return new RawContent {
                    Text = textFromImage,
                    PageNumber = content.PageNumber
                };
            });

            var textContent = await Task.WhenAll(textContentTasks).ConfigureAwait(false);

            var recordTasks = textContent.Select( async content => new TextSnippet<TKey> 
            {
                Key = uniqueKeyGenerator.GenerateKey(),
                Text = content.Text,
                ReferenceDescription = $"{new FileInfo(pdfPath).Name}#page={content.PageNumber}",
                ReferenceLink = $"{new Uri(new FileInfo(pdfPath).FullName).AbsoluteUri}#page={content.PageNumber}",
                TextEmbedding = await GenerateEmbeddingsWithRetryAsync(textEmbeddingGenerationService,
                    content.Text!,
                    cancellationToken).ConfigureAwait(false)
            }
            );

            var records = await Task.WhenAll(recordTasks).ConfigureAwait(false);

            foreach (var record in records)
            {
                var upsertedKey = await vectorStoreRecordCollection.UpsertAsync(record, cancellationToken: cancellationToken).ConfigureAwait(false);
                Console.WriteLine($"Upserted record with key: {upsertedKey}");
            }


            await Task.Delay(betweenBatchDelayInMs, cancellationToken).ConfigureAwait(false);


        }
    }


    ///<summary>
    /// 从提供的PDF文件中读取文本和图片
    /// </summary>
    /// <param name="pdfPath">需要读取的PDF文件的路径</param>
    /// <param name="cancellationToken">取消操作的Token标记</param>
    /// <returns>返回文本以及图片，并且标识文字和图片的页码</returns>
    private static IEnumerable<RawContent> LoadTextAndImages(string pdfPath, CancellationToken cancellationToken)
    {
        using (PdfDocument document = PdfDocument.Open(pdfPath))
        {
            foreach ( Page page in document.GetPages() )
            {
                if ( cancellationToken.IsCancellationRequested )
                {
                    break;
                }

                foreach ( var image in page.GetImages() )
                {
                    if ( image.TryGetPng( out var png ))
                    {
                        yield return new RawContent {
                            Image = png,
                            PageNumber = page.Number
                        };
                    }
                    else
                    {
                        Console.WriteLine($"Unsupported image format on page {page.Number}");
                    }
                }

                var blocks = DefaultPageSegmenter.Instance.GetBlocks(page.GetWords());

                foreach ( var block in blocks )
                {
                    if ( cancellationToken.IsCancellationRequested )
                    {
                        break;
                    }

                    yield return new RawContent {
                        Text = block.Text,
                        PageNumber = page.Number
                    };
                }
            }
        }
    }

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


