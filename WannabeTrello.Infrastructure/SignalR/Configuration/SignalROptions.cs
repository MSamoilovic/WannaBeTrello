namespace WannabeTrello.Infrastructure.SignalR.Configuration;

/// <summary>
/// Configuration options for the SignalR infrastructure.
/// Bind to the "SignalR" section in appsettings.json.
/// </summary>
public class SignalROptions
{
    public const string SectionName = "SignalR";

    /// <summary>Maximum size in bytes of a single incoming message (default 32 KB).</summary>
    public long MaximumReceiveMessageSize { get; set; } = 32_768;

    /// <summary>Buffer capacity for streaming invocations (default 10).</summary>
    public int StreamBufferCapacity { get; set; } = 10;

    /// <summary>When true, detailed error messages are sent to clients (disable in production).</summary>
    public bool EnableDetailedErrors { get; set; } = false;

    /// <summary>How long the server waits before considering a client timed out.</summary>
    public TimeSpan ClientTimeoutInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>How long the server waits for the initial handshake to complete.</summary>
    public TimeSpan HandshakeTimeout { get; set; } = TimeSpan.FromSeconds(15);

    /// <summary>How often the server sends keep-alive pings.</summary>
    public TimeSpan KeepAliveInterval { get; set; } = TimeSpan.FromSeconds(15);

    /// <summary>Maximum number of parallel hub method invocations per client.</summary>
    public int MaximumParallelInvocationsPerClient { get; set; } = 1;
}
