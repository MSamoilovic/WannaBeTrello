using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Domain.Entities;

public class ActivityTracker: AuditableEntity
{
    public ActivityType Type { get; init; }
    public string Description { get; init; } = null!;
    public DateTime Timestamp { get; init; }
    public long UserId { get; init; } 
    public User? User { get; init; } 
    public long? RelatedEntityId { get; init; } 
    public string? RelatedEntityType { get; init; } 
    public string? OldValue { get; init; }
    public string? NewValue { get; init; } 
}