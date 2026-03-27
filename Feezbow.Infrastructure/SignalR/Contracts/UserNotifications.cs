namespace Feezbow.Infrastructure.SignalR.Contracts;

public record UserProfileUpdatedNotification
{
    public required long UserId { get; init; }
    public required long ModifiedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record UserDeactivatedNotification
{
    public required long UserId { get; init; }
    public required long DeactivatedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record UserReactivatedNotification
{
    public required long UserId { get; init; }
    public required long ReactivatedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record UserMentionedNotification
{
    public required long MentionedUserId { get; init; }
    public required long TaskId { get; init; }
    public required long MentionedByUserId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record UserOnlineNotification
{
    public required string UserId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

public record UserOfflineNotification
{
    public required string UserId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
