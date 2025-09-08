using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Domain.Entities;

public class ActivityTracker : AuditableEntity
{
    public ActivityType Type { get; init; }
    public string Description { get; init; } = null!;
    public DateTime Timestamp { get; init; }
    public long UserId { get; init; }
    public User? User { get; init; }
    public long? RelatedEntityId { get; init; }
    public string? RelatedEntityType { get; init; }
    public Dictionary<string, object?> OldValue { get; init; } = new();
    public Dictionary<string, object?> NewValue { get; init; } = new();

    public static ActivityTracker Create(ActivityType type, string description, long userId,
        long? relatedEntityId = null, string? relatedEntityType = null, Dictionary<string, object?>? oldValue = null,
        Dictionary<string, object?>? newValue = null)
    {
        return new ActivityTracker
        {
            Type = type,
            Description = description,
            Timestamp = DateTime.UtcNow,
            UserId = userId,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType,
            OldValue = oldValue ?? new Dictionary<string, object?>(),
            NewValue = newValue ?? new Dictionary<string, object?>()
        };
    }
}