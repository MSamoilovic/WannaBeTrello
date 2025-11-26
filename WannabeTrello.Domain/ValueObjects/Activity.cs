using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Domain.ValueObjects;

public record Activity
{
    public ActivityType Type { get; init; }
    public string Description { get; init; }
    public DateTime Timestamp { get; init; }
    public long UserId { get; init; }
    public Dictionary<string, object?> OldValue { get; init; }
    public Dictionary<string, object?> NewValue { get; init; }

    private Activity() { } // EF Core constructor

    public Activity(ActivityType type, string description, long userId,
        Dictionary<string, object?>? oldValue = null,
        Dictionary<string, object?>? newValue = null)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));

        if (userId <= 0)
            throw new ArgumentException("UserId must be positive", nameof(userId));

        Type = type;
        Description = description;
        Timestamp = DateTime.UtcNow;
        UserId = userId;
        OldValue = oldValue ?? [];
        NewValue = newValue ?? [];
    }
}
