using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Specifications.UserSpecifications;

public class SearchUserSpecification: BaseSpecification<User>
{
    public SearchUserSpecification(): base(x => x.IsActive){}
}
