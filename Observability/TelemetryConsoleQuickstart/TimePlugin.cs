using System.ComponentModel;
using Microsoft.SemanticKernel;

public class TimePlugin
{
    [KernelFunction("get_time")]
    [Description("Gets the cuurent Time")]
    public async Task<string> GetTimeAsync()
    {
        await Task.CompletedTask;
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

}