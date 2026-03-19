using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;

namespace Feezbow.Infrastructure.SignalR.Resilience;

public abstract class ResilientNotificationBase(ResiliencePipeline pipeline, ILogger logger)
{
    protected async Task SendAsync(
        Func<CancellationToken, ValueTask> action,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await pipeline.ExecuteAsync(action, cancellationToken);
        }
        catch (BrokenCircuitException)
        {
            logger.LogWarning("SignalR circuit breaker is open – notification skipped");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SignalR notification failed after all retries");
        }
    }
}
