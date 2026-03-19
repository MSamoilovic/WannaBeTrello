using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Specifications.UserSpecifications;

public class SearchUserSpecification: BaseSpecification<User>
{
    public SearchUserSpecification(): base(x => x.IsActive){}
}
