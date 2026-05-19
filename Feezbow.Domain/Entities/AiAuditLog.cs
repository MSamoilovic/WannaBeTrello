namespace Feezbow.Domain.Entities;

public class AiAuditLog
{
    public long Id { get; private set; }
    public long UserId { get; private set; }
    public string AgentName { get; private set; } = string.Empty;
    public string InputHash { get; private set; } = string.Empty; // SHA-256 of input, not raw text
    public int InputTokens { get; private set; }
    public int OutputTokens { get; private set; }
    public bool CacheHit { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public bool? WasAccurate { get; private set; }
    public DateTimeOffset? FeedbackReceivedAt { get; private set; }

    private AiAuditLog() { }

    public static AiAuditLog Create(
        long userId,
        string agentName,
        string inputHash,
        int inputTokens,
        int outputTokens,
        bool cacheHit
    ) =>
        new()
        {
            UserId = userId,
            AgentName = agentName,
            InputHash = inputHash,
            InputTokens = inputTokens,
            OutputTokens = outputTokens,
            CacheHit = cacheHit,
            CreatedAt = DateTimeOffset.UtcNow,
        };

    public void RecordFeedback(bool wasAccurate)
    {
        WasAccurate = wasAccurate;
        FeedbackReceivedAt = DateTimeOffset.UtcNow;
    }
}
