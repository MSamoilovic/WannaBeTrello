using WannabeTrello.Domain.ValueObjects;

namespace WannabeTrello.Application.Features.Activities.GetActivityByProject;

public class GetActivityByProjectQueryResponse
{
    public string? Type { get; init; }
    public string? Description { get; init; }
    public DateTime Timestamp { get; init; }
    public long UserId { get; init; }
    public Dictionary<string, object?> OldValue { get; init; } = [];
    public Dictionary<string, object?> NewValue { get; init; } = [];

    public static GetActivityByProjectQueryResponse FromEntity(Activity activity) => new()
    {
        Type = activity.Type.ToString(),
        Description = activity.Description,
        Timestamp = activity.Timestamp,
        UserId = activity.UserId,
        OldValue = activity.OldValue,
        NewValue = activity.NewValue,
    };
}
