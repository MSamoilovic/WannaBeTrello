namespace WannabeTrello.Infrastructure.SignalR.Security;

/// <summary>
/// Controls the rate of hub method invocations per user.
/// </summary>
public interface IRateLimiter
{
    /// <summary>
    /// Returns <c>true</c> if the request is allowed within the sliding window;
    /// <c>false</c> if the caller has exceeded <paramref name="maxRequests"/>.
    /// </summary>
    bool IsAllowed(string key, int maxRequests, TimeSpan window);
}
