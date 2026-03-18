using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Specifications.UserSpecifications;

public class GetUserProfileSpecification(long userId) : BaseSpecification<User>(x => x.Id == userId);