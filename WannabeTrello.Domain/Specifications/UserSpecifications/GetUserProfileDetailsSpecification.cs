using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Specifications.UserSpecifications;

public class GetUserProfileDetailsSpecification: BaseSpecification<User>
{
    public GetUserProfileDetailsSpecification(long userId) : base(x => x.Id == userId)
    {
        AddInclude(x => x.AssignedTasks);
        AddInclude(x => x.OwnedProjects);
        AddInclude(x => x.ProjectMemberships);
        AddInclude(x => x.BoardMemberships);
    }
}