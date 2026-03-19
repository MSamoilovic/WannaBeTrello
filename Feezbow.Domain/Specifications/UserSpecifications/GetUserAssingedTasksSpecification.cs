using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Specifications.UserSpecifications;

public class GetUserAssingedTasksSpecification: BaseSpecification<BoardTask>
{
    public GetUserAssingedTasksSpecification(long userId): base(x => x.AssigneeId == userId) { }
}
