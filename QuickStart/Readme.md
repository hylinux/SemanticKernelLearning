# Quick Start

I have used the pakcage "CommandLine" for command line parsing. You can try this code by this:

```bash
dotnet run - -m "gpt-4.1" -k "{your Key}" -d "{your endpoint url}" -l "None"
```

and the parameters are:

```powershell
  Required option 'm, modelId' is missing.
  Required option 'k, apiKey' is missing.
  Required option 'd, endpoint' is missing.

  -m, --modelId     Required. The AI Model Id, e.g., gpt-3.5-turbo, gpt-4, etc.

  -k, --apiKey      Required. The OpenAI API Key.

  -d, --endpoint    Required. The OpenAI API Endpoint. you can use other endpoints, such as Azure OpenAI Service.

  -l, --logLevel    (Default: Trace) The log level, e.g., Trace, Debug, Information, Warning, Error, Critical.

  --help            Display this help screen.

  --version         Display version information.

```
