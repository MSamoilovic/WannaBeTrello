

using WannabeTrello.Domain.ValueObjects;

namespace WannabeTrello.Application.Features.Activities.GetActivityByBoard
{
    public class GetActivityByBoardQueryResponse
    {
        public string? Type { get; private set; }
        public string? Description { get; private set; }
        public DateTime Timestamp { get; private set; }
        public long UserId { get; private set; }
        public Dictionary<string, object?> OldValue { get; private set; } = [];
        public Dictionary<string, object?> NewValue { get; private set; } = [];

        public static GetActivityByBoardQueryResponse FromEntity(Activity activity) => new()
        {
            Type = activity.Type.ToString(),
            Description = activity.Description,
            Timestamp = activity.Timestamp,
            UserId = activity.UserId,
            OldValue = activity.OldValue,
            NewValue = activity.NewValue,
        };
    }
}
