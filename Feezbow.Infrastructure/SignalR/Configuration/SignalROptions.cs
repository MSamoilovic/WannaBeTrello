namespace Feezbow.Infrastructure.SignalR.Configuration;

public class SignalROptions
{
    public const string SectionName = "SignalR";

    public long MaximumReceiveMessageSize { get; set; } = 32_768;
    public int StreamBufferCapacity { get; set; } = 10;
    public bool EnableDetailedErrors { get; set; } = false;
    public TimeSpan ClientTimeoutInterval { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan HandshakeTimeout { get; set; } = TimeSpan.FromSeconds(15);
    public TimeSpan KeepAliveInterval { get; set; } = TimeSpan.FromSeconds(15);
    public int MaximumParallelInvocationsPerClient { get; set; } = 1;
    public SignalRRateLimitOptions RateLimiting { get; set; } = new();
}

public class SignalRRateLimitOptions
{
    public bool Enabled { get; set; } = true;
    public int MaxRequestsPerSecond { get; set; } = 10;
    public int MaxRequestsPerMinute { get; set; } = 100;
}
