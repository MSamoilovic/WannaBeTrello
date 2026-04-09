using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Feezbow.Infrastructure.SignalR.Configuration;
using Feezbow.Infrastructure.SignalR.Security;

namespace Feezbow.Infrastructure.SignalR.Filters;

/// <summary>
/// Hub filter that enforces per-user, per-method rate limiting.
/// Runs between <see cref="Feezbow.Infrastructure.SignalR.Hubs.Base.HubMethodFilter"/> (outer)
/// and <see cref="HubExceptionFilter"/> (inner).
/// </summary>
public sealed class HubRateLimitingFilter(
    IRateLimiter rateLimiter,
    IOptions<SignalROptions> options,
    ILogger<HubRateLimitingFilter> logger)
    : IHubFilter
{
    private readonly SignalRRateLimitOptions _rateLimitOptions = options.Value.RateLimiting;

    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        if (!_rateLimitOptions.Enabled)
            return await next(invocationContext);

        // Key is per-user per-method so each method has its own allowance
        var userId = invocationContext.Context.UserIdentifier
                     ?? invocationContext.Context.ConnectionId;
        var method = invocationContext.HubMethodName;

        if (!rateLimiter.IsAllowed($"rl:s:{userId}:{method}", _rateLimitOptions.MaxRequestsPerSecond, TimeSpan.FromSeconds(1)))
        {
            logger.LogWarning("Per-second rate limit exceeded for user {UserId} on hub method {HubMethod}", userId, method);
            throw new HubException("Rate limit exceeded. Please slow down.");
        }

        if (!rateLimiter.IsAllowed($"rl:m:{userId}:{method}", _rateLimitOptions.MaxRequestsPerMinute, TimeSpan.FromMinutes(1)))
        {
            logger.LogWarning("Per-minute rate limit exceeded for user {UserId} on hub method {HubMethod}", userId, method);
            throw new HubException("Rate limit exceeded. Please slow down.");
        }

        return await next(invocationContext);
    }
}
