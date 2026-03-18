using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Domain.ValueObjects;

public class Activity
{
    public ActivityType Type { get; private set; }
    public string Description { get; private set; }
    public DateTime Timestamp { get; private set; }
    public long UserId { get; private set; }
    public Dictionary<string, object?> OldValue { get; private set; }
    public Dictionary<string, object?> NewValue { get; private set; }

    private Activity() 
    {
        
        Description = string.Empty;
        OldValue = [];
        NewValue = [];
    }

    public Activity(
        ActivityType type, 
        string description, 
        long userId,
        Dictionary<string, object?>? oldValue = null,
        Dictionary<string, object?>? newValue = null
    )
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
