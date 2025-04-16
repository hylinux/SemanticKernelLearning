using CommandLine;

internal class Parameters
{
    //大模型访问的模型ID
    [Option('m',
            "modelId",
            Required = true,
            HelpText = "The AI Model Id, e.g., gpt-3.5-turbo, gpt-4, etc.")]
    public string? ModelId { get; set; }

    //大模型访问的key
    [Option('k',
            "apiKey",
            Required = true,
            HelpText = "The OpenAI API Key.")]
    public string? ApiKey { get; set; }

    //大模型访问的请求endpoint
    [Option('d',
            "endpoint",
            Required = true,
            HelpText = "The OpenAI API Endpoint. you can use other endpoints, such as Azure OpenAI Service.")]
    public string? Endpoint { get; set; }



    //配置脚本的日志级别
    [Option('l',
            "logLevel",
            Required = false,
            Default = "Trace",
            HelpText = "The log level, e.g., Trace, Debug, Information, Warning, Error, Critical.")]
    public string? LogLevel { get; set; }



}