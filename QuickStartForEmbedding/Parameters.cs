using CommandLine;

internal class Parameters
{
    [Option('d',
        "deployment",
        Required = true,
        HelpText = "The deployment name.")]
    public string? DeploymentName {get; set;}
    

    [Option('m',
        "model",
        Required = true,
        HelpText = "The model to use. Options: gpt-3.5-turbo, gpt-4, gpt-4-32k, gpt-4-turbo, gpt-4-turbo-32k, gpt-4-turbo-16k, gpt-3.5-turbo-16k, gpt-3.5-turbo-32k, gpt-3.5-turbo-instruct, gpt-3.5-turbo-instruct-16k, gpt-3.5-turbo-instruct-32k" +
            " (default: gpt-3.5-turbo)")]
    public string? ModelId {get; set;}

    [Option('k',
        "apiKey",
        Required = true,
        HelpText = "The OpenAI API key.")]
    public string? ApiKey {get; set;}

    [Option('n',
        "endpoint",
        Required = true,
        HelpText = "The OpenAI API endpoint.")]
    public string? Endpoint {get; set;}


    [Option('l',
        "logLevel",
        Required = true,
        HelpText = "The log level. Options: Trace, Debug, Information, Warning, Error, Critical, None (default: Information)")]
    public string? LogLevel {get; set;}


    [Option('t', "text",
        Required = true,
        HelpText = "The text to generate an embedding for.")]
    public string? EmbeddingText {get; set;}


}