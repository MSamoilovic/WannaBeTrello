using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace WannabeTrello.Infrastructure.SignalR.Hubs.Base;

/// <summary>
/// Global hub filter that provides structured logging, error handling,
/// and audit trail for all hub method invocations.
/// </summary>
public sealed class HubMethodFilter(ILogger<HubMethodFilter> logger) : IHubFilter
{
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        var method = invocationContext.HubMethodName;
        var connectionId = invocationContext.Context.ConnectionId;
        var userId = invocationContext.Context.UserIdentifier ?? "anonymous";

        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["HubMethod"] = method,
            ["ConnectionId"] = connectionId,
            ["UserId"] = userId
        }))
        {
            logger.LogDebug("Invoking hub method {HubMethod} for user {UserId}", method, userId);

            var auditAttr = invocationContext.HubMethod
                .GetCustomAttributes(typeof(HubMethodAttribute), false)
                .OfType<HubMethodAttribute>()
                .FirstOrDefault();

            if (auditAttr?.RequiresAudit == true)
            {
                logger.LogInformation(
                    "[AUDIT] Hub method {HubMethod} called by user {UserId} on connection {ConnectionId}",
                    method, userId, connectionId);
            }

            try
            {
                return await next(invocationContext);
            }
            catch (HubException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Unhandled exception in hub method {HubMethod} for user {UserId}",
                    method, userId);
                throw new HubException("An unexpected error occurred.");
            }
        }
    }
}
