using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Specifications.ActivitySpecifications;

public class GetActivityByProjectIdSpecification: BaseSpecification<ActivityLog>
{
    public GetActivityByProjectIdSpecification(long projectId): base(x => x.ProjectId == projectId)
    {
        ApplyOrderByDescending(x => x.Activity.Timestamp);
    }
}
