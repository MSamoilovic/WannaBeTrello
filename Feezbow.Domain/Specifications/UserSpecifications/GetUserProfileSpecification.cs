using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Specifications.UserSpecifications;

public class GetUserProfileSpecification(long userId) : BaseSpecification<User>(x => x.Id == userId);