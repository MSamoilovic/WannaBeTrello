using Feezbow.Domain.Enums;

namespace Feezbow.Infrastructure.SignalR.Contracts;

public record HouseholdProfileCreatedNotification
{
    public required long HouseholdId { get; init; }
    public required long ProjectId { get; init; }
    public required long CreatedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record HouseholdProfileUpdatedNotification
{
    public required long HouseholdId { get; init; }
    public required long ProjectId { get; init; }
    public required long ModifiedBy { get; init; }
    public required IReadOnlyDictionary<string, object?> Changes { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record HouseholdMemberRoleAssignedNotification
{
    public required long ProjectId { get; init; }
    public required long MemberId { get; init; }
    public required HouseholdRole Role { get; init; }
    public HouseholdRole? PreviousRole { get; init; }
    public required long AssignedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record HouseholdMemberRoleRemovedNotification
{
    public required long ProjectId { get; init; }
    public required long MemberId { get; init; }
    public required HouseholdRole PreviousRole { get; init; }
    public required long RemovedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}
