using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using CommandLine;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;



//从命令行参数中获取参数
Parameters? parameters = null;

ParserResult<Parameters> parserResult = Parser.Default.ParseArguments<Parameters>(args)
            .WithParsed<Parameters>(r => parameters = r)
            .WithNotParsed<Parameters>(errors =>
            {
                Environment.Exit(1);
            });

//从命令行参数中拿到您需要的三个值，用于向大模型发送请求
var modelId = parameters?.ModelId ?? "gpt-3.5-turbo";
var endpoint = parameters?.Endpoint ?? "https://api.openai.com/v1/chat/completions";
var apiKey = parameters?.ApiKey;
var logLevel = parameters?.LogLevel ?? "Trace";

// Create a kernel with Azure OpenAI chat completion
var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(modelId, endpoint, apiKey!);

var logLevelEnum = Enum.TryParse(logLevel, true, out LogLevel logLevelValue) ? logLevelValue : LogLevel.Trace;


// Add enterprise components
builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(logLevelEnum));


//开始配置OpenTelemetry
var resourceBuilder = ResourceBuilder
    .CreateDefault()
    .AddService("TelemetryConsoleQuickstart");

// Enable model diagnostics with sensitive data.
AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true);

using var traceProvider = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(resourceBuilder)
    .AddSource("Microsoft.SemanticKernel*")
    .AddConsoleExporter()
    .Build();

using var meterProvider = Sdk.CreateMeterProviderBuilder()
    .SetResourceBuilder(resourceBuilder)
    .AddMeter("Microsoft.SemanticKernel*")
    .AddConsoleExporter()
    .Build();

builder.Services.AddLogging(
builder =>
{
    // Add OpenTelemetry as a logging provider
    builder.AddOpenTelemetry(options =>
    {
        options.SetResourceBuilder(resourceBuilder);
        options.AddConsoleExporter();
        // Format log messages. This is default to false.
        options.IncludeFormattedMessage = true;
        options.IncludeScopes = true;
    });
    builder.SetMinimumLevel(LogLevel.Information);
}
);








// Build the kernel
Kernel kernel = builder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Add a plugin (the LightsPlugin class is defined below)
kernel.Plugins.AddFromType<LightsPlugin>("Lights");
kernel.Plugins.AddFromType<TimePlugin>("Time");
kernel.Plugins.AddFromType<LocatePlugin>("Locate");



// Enable planning
OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

// Create a history store the conversation
var history = new ChatHistory();

// Initiate a back-and-forth chat
string? userInput;
do
{
    // Collect user input
    Console.Write("User > ");
    userInput = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userInput))
    {
        break;
    }

    // Add user input
    history.AddUserMessage(userInput);

    // Get the response from the AI
    var result = await chatCompletionService.GetChatMessageContentAsync(
        history,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel);

    // Print the results
    Console.WriteLine("Assistant > " + result);

    // Add the message from the agent to the chat history
    history.AddMessage(result.Role, result.Content ?? string.Empty);
} while (userInput is not null);