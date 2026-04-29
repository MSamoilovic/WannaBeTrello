namespace Feezbow.Infrastructure.SignalR.Contracts;

public record ChoreCreatedNotification
{
    public required long ChoreId { get; init; }
    public required long ProjectId { get; init; }
    public required string Title { get; init; }
    public long? AssignedToUserId { get; init; }
    public required long CreatedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record ChoreUpdatedNotification
{
    public required long ChoreId { get; init; }
    public required long ProjectId { get; init; }
    public required long ModifiedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record ChoreAssignedNotification
{
    public required long ChoreId { get; init; }
    public required long ProjectId { get; init; }
    public long? AssignedToUserId { get; init; }
    public long? PreviousUserId { get; init; }
    public required long AssignedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record ChoreCompletedNotification
{
    public required long ChoreId { get; init; }
    public required long ProjectId { get; init; }
    public required long CompletedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record ChoreDeletedNotification
{
    public required long ChoreId { get; init; }
    public required long ProjectId { get; init; }
    public required long DeletedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}
