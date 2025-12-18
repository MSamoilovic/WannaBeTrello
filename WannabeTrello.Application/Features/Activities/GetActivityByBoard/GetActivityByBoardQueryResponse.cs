

using System.Collections.Immutable;
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

        public static IReadOnlyList<GetActivityByBoardQueryResponse> FromEntity(IEnumerable<Activity> activity)
        {
            return activity.Select(act => new GetActivityByBoardQueryResponse
            {
                Type = act.Type.ToString(),
                Description = act.Description,
                Timestamp = act.Timestamp,
                UserId = act.UserId,
                OldValue = act.OldValue,
                NewValue = act.NewValue,
            }).ToImmutableList();
        }
    }
}
