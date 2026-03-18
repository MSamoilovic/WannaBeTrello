using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Specifications.UserSpecifications;

public class GetUserAssingedTasksSpecification: BaseSpecification<BoardTask>
{
    public GetUserAssingedTasksSpecification(long userId): base(x => x.AssigneeId == userId) { }
}
