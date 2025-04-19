using CommandLine;

internal class Parameters
{

    //大模型的模型id
    [Option('m', "model",
        Required = true,
        HelpText = "The model id of the large model.")]
    public string? ModelId { get; set; }

    [Option('k', 
        "apiKey",
        Required = true,
        HelpText = "The API key for the large model.")]
    public string? ApiKey { get; set; }

    //大模型访问的endpoint
    [Option('d',
        "endpoint",
        Required = true,
        HelpText = "The endpoint for the large model.")]
    public string? Endpoint { get; set; }

    //配置脚本的日志级别
    [Option('l',
            "logLevel",
            Required = false,
            Default = "Trace",
            HelpText = "The log level, e.g., Trace, Debug, Information, Warning, Error, Critical.")]
    public string? LogLevel { get; set; }


}