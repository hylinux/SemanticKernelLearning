using System.ComponentModel;
using Microsoft.SemanticKernel;

public class LocatePlugin
{
    [KernelFunction("get_location")]
    [Description("Gets the current location")]
    public async Task<string> GetLocationAsync()
    {
        await Task.CompletedTask;
        return "You are in Shanghai, China.";
    }

    [KernelFunction("get_weather")]
    [Description("Gets the current weather")]
    public async Task<string> GetWeatherAsync()
    {
        await Task.CompletedTask;
        return "The weather is sunny with a temperature of 25°C.";
    }
}

