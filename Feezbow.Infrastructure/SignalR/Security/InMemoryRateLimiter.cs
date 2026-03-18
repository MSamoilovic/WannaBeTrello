using System.Collections.Concurrent;

namespace WannabeTrello.Infrastructure.SignalR.Security;

public sealed class InMemoryRateLimiter : IRateLimiter
{
    private readonly ConcurrentDictionary<string, Queue<long>> _windows = new();

    public bool IsAllowed(string key, int maxRequests, TimeSpan window)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var cutoff = now - (long)window.TotalMilliseconds;

        var queue = _windows.GetOrAdd(key, _ => new Queue<long>());

        lock (queue)
        {
            // Evict timestamps that have fallen outside the window
            while (queue.Count > 0 && queue.Peek() < cutoff)
                queue.Dequeue();

            if (queue.Count >= maxRequests)
                return false;

            queue.Enqueue(now);
            return true;
        }
    }
}
