using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces.Repositories;
using WannabeTrello.Domain.Specifications.UserSpecifications;

namespace WannabeTrello.Infrastructure.Persistence.Repositories;

public class UserRepository(ApplicationDbContext dbContext) : Repository<User>(dbContext), IUserRepository
{
    public Task<User?> GetUserProfileDetailsAsync(long userId, CancellationToken cancellationToken)
    {
        var specification = new GetUserProfileDetailsSpecification(userId);
        return GetSingleAsync(specification, cancellationToken);
    }

    public Task<User?> GetUserProfileAsync(long userId, CancellationToken cancellationToken)
    {
       var specification = new GetUserProfileSpecification(userId);
       return GetSingleAsync(specification, cancellationToken);
    }

    public IQueryable<User> SearchUsers()
    {
        var specification = new SearchUserSpecification();
        return ApplySpecification(specification);
    }
}