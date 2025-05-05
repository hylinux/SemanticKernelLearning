using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using VectorStoreRag.Options;


namespace VectorStoreRag;


/// <summary>
/// RAG应用类的主要服务类
/// </summary>
/// <typeparam name="TKey">向量数据库的键类型</typeparam>
/// <param name="dataLoader">数据加载器</param>
/// <param name="vectorStoreTextSearch">向量数据库文本搜索服务</param>
/// <param name="kerenel">SK内核</param>
/// <param name="ragConfigOptions">RAG配置选项</param>
/// <param name="appShutdownCancellationToken">应用关闭的Cancellation Token</param>
/// <remarks>该类实现了IRagService接口，提供了RAG应用的主要功能。</remarks>

internal sealed class RAGChatService<Tkey>(
    IDataLoader dataLoader,
    VectorStoreTextSearch<TextSnippet<Tkey>> vectorStoreTextSearch,
    Kernel kernel,
    IOptions<RagConfig> ragConfigOptions,
    [FromKeyedServices("AppShutdown")] CancellationTokenSource appShutdownCancellationTokenSource) : IHostedService
{

    private Task? _dataLoaded;
    private Task? _chatLoop;




    /// <summary>
    /// 启动服务    
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    /// <remarks>该方法实现了IHostedService接口的StartAsync方法，启动服务并加载数据。</remarks>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if ( ragConfigOptions.Value.BuildCollection )
        {
            this._dataLoaded = this.LoadDataAsync(cancellationToken);
        }
        else
        {
            this._dataLoaded = Task.CompletedTask;
        }


        this._chatLoop = this.ChatLoopAsync(cancellationToken);

        return Task.CompletedTask;
    }


    /// <summary>
    /// 停止服务
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// 
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }


    private async Task ChatLoopAsync(CancellationToken cancellationToken)
    {
        var pdfFiles = string.Join(",", ragConfigOptions.Value.PdfFilePaths ?? [] );

        while ( this._dataLoaded != null && !this._dataLoaded.IsCompleted && !cancellationToken.IsCancellationRequested )
        {
            await Task.Delay(1_000, cancellationToken).ConfigureAwait(false);
        } 

        if ( this._dataLoaded != null && this._dataLoaded.IsFaulted )
        {
            Console.WriteLine("Failed to load data");
            return;
        }

        Console.WriteLine("PDF loading completed\n");

        Console.ForegroundColor = ConsoleColor.Green;

        Console.WriteLine("Assistant > Press enter with no prompt to exit.");


        kernel.Plugins.Add(vectorStoreTextSearch.CreateWithGetTextSearchResults("SearchPlugin"));

        while ( !cancellationToken.IsCancellationRequested )
        {
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine($"Assistant > What would you like to know from the loaded PDFs: ({pdfFiles})?");

            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("User > ");

            var question = Console.ReadLine();

            if ( string.IsNullOrWhiteSpace(question) )
            {
                appShutdownCancellationTokenSource.Cancel();
                break;
            }


            //调用大模型
            var response = kernel.InvokePromptStreamingAsync(
                promptTemplate: """
                     Please use this information to answer the question:
                    {{#with (SearchPlugin-GetTextSearchResults question)}}  
                      {{#each this}}  
                        Name: {{Name}}
                        Value: {{Value}}
                        Link: {{Link}}
                        -----------------
                      {{/each}}
                    {{/with}}

                    Include citations to the relevant information where it is referenced in the response.
                    
                    Question: {{question}}                   
                """,
                arguments: new KernelArguments()
                {
                    {"question", question },
                },

                templateFormat: "handlebars",
                promptTemplateFactory: new HandlebarsPromptTemplateFactory(),
                cancellationToken: cancellationToken
            );


            // Stream the LLM response to the console with error handling.
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\nAssistant > ");

            try
            {
                await foreach (var message in response.ConfigureAwait(false))
                {
                    Console.Write(message);
                }
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Call to LLM failed with error: {ex}");
            }
        }

    }



    /// <summary>
    /// Load all configured PDFs into the vector store.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
    /// <returns>An async task that completes when the loading is complete.</returns>
    private async Task LoadDataAsync(CancellationToken cancellationToken)
    {
        try
        {
            foreach (var pdfFilePath in ragConfigOptions.Value.PdfFilePaths ?? [])
            {
                Console.WriteLine($"Loading PDF into vector store: {pdfFilePath}");
                await dataLoader.LoadPdf(
                    pdfFilePath,
                    ragConfigOptions.Value.DataLoadingBatchSize,
                    ragConfigOptions.Value.DataLoadingBetweenBatchDelayInMilliseconds,
                    cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load PDFs: {ex}");
            throw;
        }
    }

}
