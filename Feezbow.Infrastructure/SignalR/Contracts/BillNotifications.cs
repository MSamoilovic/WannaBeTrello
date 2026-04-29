namespace Feezbow.Infrastructure.SignalR.Contracts;

public record BillCreatedNotification
{
    public required long BillId { get; init; }
    public required long ProjectId { get; init; }
    public required string Title { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required long CreatedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record BillUpdatedNotification
{
    public required long BillId { get; init; }
    public required long ProjectId { get; init; }
    public required long ModifiedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record BillPaidNotification
{
    public required long BillId { get; init; }
    public required long ProjectId { get; init; }
    public required decimal Amount { get; init; }
    public required long PaidBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record BillSplitPaidNotification
{
    public required long BillId { get; init; }
    public required long ProjectId { get; init; }
    public required long UserId { get; init; }
    public required decimal Amount { get; init; }
    public required long PaidBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record BillDeletedNotification
{
    public required long BillId { get; init; }
    public required long ProjectId { get; init; }
    public required long DeletedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}
