using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Specifications.ActivitySpecifications;

public class GetActivityByTaskIdSpecification: BaseSpecification<ActivityLog>
{
    public GetActivityByTaskIdSpecification(long boardTaskId): base(x => x.BoardTaskId == boardTaskId)
    {
        ApplyOrderByDescending(x => x.Activity.Timestamp);
    }
}
