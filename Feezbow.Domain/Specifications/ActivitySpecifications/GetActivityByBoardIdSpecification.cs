using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Specifications.ActivitySpecifications;

public class GetActivityByBoardIdSpecification: BaseSpecification<ActivityLog>
{
    public GetActivityByBoardIdSpecification(long boardId): base(x => x.BoardId == boardId)
    {
        ApplyOrderByDescending(x => x.Activity.Timestamp);
    }
}
