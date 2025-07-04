using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Domain.Entities;

public class ActivityTracker: AuditableEntity
{
    public Guid Id { get; set; }
    public ActivityType ActivityType { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}