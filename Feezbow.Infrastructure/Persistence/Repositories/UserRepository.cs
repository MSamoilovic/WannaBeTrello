using Microsoft.EntityFrameworkCore;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Interfaces.Repositories;
using Feezbow.Domain.Specifications.UserSpecifications;

namespace Feezbow.Infrastructure.Persistence.Repositories;

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

    public async Task<IReadOnlyList<Project>> GetUserOwnedProjectsAsync(long userId, CancellationToken cancellationToken)
    {
       var spec = new GetUserOwnedProjectsSpecification(userId);
       var query = SpecificationQueryBuilder.GetQuery(_dbContext.Projects, spec);

       return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> GetUserProjectsAsync(long userId, CancellationToken cancellationToken)
    {
        var spec = new GetUserProjectsSpecification(userId);
        var query = SpecificationQueryBuilder.GetQuery(_dbContext.Projects, spec);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Board>> GetUserBoardsAsync(long userId, CancellationToken cancellationToken)
    {
        var spec = new GetUserBoardsSpecifications(userId);
        var query = SpecificationQueryBuilder.GetQuery(_dbContext.Boards, spec);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BoardTask>> GetUserAssignedTasksAsync(long userId, CancellationToken cancellationToken)
    {
        var spec = new GetUserAssingedTasksSpecification(userId);
        var query = SpecificationQueryBuilder.GetQuery(_dbContext.Tasks, spec);

        return await query.ToListAsync(cancellationToken);
    }
}