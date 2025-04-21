using Microsoft.SemanticKernel;

public sealed class TestAutoFuncFilter : IAutoFunctionInvocationFilter
{
    public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
    {
        await next(context);

        if (context.Function.Name == "get_lights")
        {
            var data = context.Result.GetValue<string>();
            Console.WriteLine($"Current Data: {data}");


        }

        
    }


}

