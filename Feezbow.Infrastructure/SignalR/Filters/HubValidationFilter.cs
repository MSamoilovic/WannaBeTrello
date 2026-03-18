using Microsoft.AspNetCore.SignalR;

namespace WannabeTrello.Infrastructure.SignalR.Filters;

/// <summary>
/// Hub filter that validates hub method arguments before the method executes.
/// <list type="bullet">
///   <item><description>Numeric IDs (<c>long</c>, <c>int</c>) must be &gt; 0.</description></item>
///   <item><description>String parameters must not be null or whitespace.</description></item>
/// </list>
/// </summary>
public sealed class HubValidationFilter : IHubFilter
{
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        var parameters = invocationContext.HubMethod.GetParameters();
        var arguments = invocationContext.HubMethodArguments;

        for (var i = 0; i < parameters.Length && i < arguments.Count; i++)
        {
            var param = parameters[i];
            var value = arguments[i];

            switch (value)
            {
                case long id when id <= 0:
                    throw new HubException(
                        $"Invalid value for '{param.Name}': must be a positive ID.");

                case int id when id <= 0:
                    throw new HubException(
                        $"Invalid value for '{param.Name}': must be a positive ID.");

                case string str when string.IsNullOrWhiteSpace(str):
                    throw new HubException(
                        $"Parameter '{param.Name}' cannot be empty.");
            }
        }

        return await next(invocationContext);
    }
}
