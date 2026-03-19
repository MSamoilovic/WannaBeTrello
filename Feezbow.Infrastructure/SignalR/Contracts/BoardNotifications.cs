namespace Feezbow.Infrastructure.SignalR.Contracts;

public record BoardCreatedNotification
{
    public required long BoardId { get; init; }
    public required string BoardName { get; init; }
    public required long CreatedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record BoardUpdatedNotification
{
    public required long BoardId { get; init; }
    public required long ModifiedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record BoardArchivedNotification
{
    public required long BoardId { get; init; }
    public required long ArchivedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record BoardRestoredNotification
{
    public required long BoardId { get; init; }
    public required long RestoredBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record ColumnCreatedNotification
{
    public required long BoardId { get; init; }
    public required long ColumnId { get; init; }
    public required string ColumnName { get; init; }
    public required long CreatedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record ColumnUpdatedNotification
{
    public required long BoardId { get; init; }
    public required long ColumnId { get; init; }
    public required string OldName { get; init; }
    public required string NewName { get; init; }
    public required long ModifiedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record ColumnOrderChangedNotification
{
    public required long BoardId { get; init; }
    public required long ColumnId { get; init; }
    public required int OldOrder { get; init; }
    public required int NewOrder { get; init; }
    public required long ModifiedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record ColumnWipLimitChangedNotification
{
    public required long BoardId { get; init; }
    public required long ColumnId { get; init; }
    public int? OldWipLimit { get; init; }
    public int? NewWipLimit { get; init; }
    public required long ModifiedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record ColumnDeletedNotification
{
    public required long BoardId { get; init; }
    public required long ColumnId { get; init; }
    public required long DeletedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record TaskCreatedNotification
{
    public required long TaskId { get; init; }
    public required string TaskTitle { get; init; }
    public required long BoardId { get; init; }
    public required long CreatedBy { get; init; }
    public long? AssigneeId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record TaskUpdatedNotification
{
    public required long TaskId { get; init; }
    public required long BoardId { get; init; }
    public required long ModifiedBy { get; init; }
    public IReadOnlyDictionary<string, object?> Changes { get; init; } = new Dictionary<string, object?>();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record TaskMovedNotification
{
    public required long TaskId { get; init; }
    public required long BoardId { get; init; }
    public required long NewColumnId { get; init; }
    public required long MovedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record TaskAssignedNotification
{
    public required long TaskId { get; init; }
    public required long BoardId { get; init; }
    public long? OldAssigneeId { get; init; }
    public long? NewAssigneeId { get; init; }
    public required long AssignedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record CommentAddedNotification
{
    public required long TaskId { get; init; }
    public required long CommentId { get; init; }
    public required long AuthorId { get; init; }
    public required string Content { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record CommentUpdatedNotification
{
    public required long TaskId { get; init; }
    public required long CommentId { get; init; }
    public required long ModifiedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record CommentDeletedNotification
{
    public required long TaskId { get; init; }
    public required long CommentId { get; init; }
    public required long DeletedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record CommentRestoredNotification
{
    public required long TaskId { get; init; }
    public required long CommentId { get; init; }
    public required long RestoredBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}
