using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Specifications.ActivitySpecifications;

public class GetActivityByUserIdSpecification: BaseSpecification<ActivityLog>
{
    public GetActivityByUserIdSpecification(long userId) : base(x => x.Activity.UserId == userId)
    {
        ApplyOrderByDescending(x => x.Activity.Timestamp);
    }
}
