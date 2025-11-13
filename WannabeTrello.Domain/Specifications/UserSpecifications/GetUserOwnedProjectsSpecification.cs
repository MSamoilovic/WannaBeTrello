using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Specifications.UserSpecifications;

public class GetUserOwnedProjectsSpecification: BaseSpecification<Project>
{
    public GetUserOwnedProjectsSpecification(long userId) : base(x => x.OwnerId == userId) { }
}
