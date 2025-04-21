using Microsoft.SemanticKernel;

public sealed class LoggingFilter : IFunctionInvocationFilter
{

    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        Console.WriteLine($"Fucntion Invocation Name:  {context.Function.PluginName}.{context.Function.Name}");

        await next(context);

        Console.WriteLine($"Function Invoked:  {context.Function.PluginName}.{context.Function.Name}");
        Console.WriteLine($"Function Result:  {context.Result}");
    }
}