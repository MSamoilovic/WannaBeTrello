namespace WannabeTrello.Infrastructure.SignalR.Contracts;

public record ProjectCreatedNotification
{
    public required long ProjectId { get; init; }
    public required string ProjectName { get; init; }
    public required long CreatedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record ProjectUpdatedNotification
{
    public required long ProjectId { get; init; }
    public required long ModifiedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record ProjectArchivedNotification
{
    public required long ProjectId { get; init; }
    public required long ArchivedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record ProjectMemberAddedNotification
{
    public required long ProjectId { get; init; }
    public required long MemberId { get; init; }
    public required long AddedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record ProjectMemberRemovedNotification
{
    public required long ProjectId { get; init; }
    public required long MemberId { get; init; }
    public required long RemovedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record ProjectMemberUpdatedNotification
{
    public required long ProjectId { get; init; }
    public required long MemberId { get; init; }
    public required long ModifiedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}
