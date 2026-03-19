using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Specifications.UserSpecifications;

public class GetUserOwnedProjectsSpecification: BaseSpecification<Project>
{
    public GetUserOwnedProjectsSpecification(long userId) : base(x => x.OwnerId == userId) { }
}
