using Microsoft.EntityFrameworkCore;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Interfaces.Repositories;
using Feezbow.Domain.Specifications.ActivitySpecifications;

namespace Feezbow.Infrastructure.Persistence.Repositories;

public class ActivityLogRepository(ApplicationDbContext dbContext)
    : Repository<ActivityLog>(dbContext), IActivityLogRepository
{
    public Task<IReadOnlyList<ActivityLog>> GetByBoardIdAsync(long boardId, CancellationToken cancellationToken = default)
    {
        var specification = new GetActivityByBoardIdSpecification(boardId);
        return GetAsync(specification, cancellationToken);
    }

    public Task<IReadOnlyList<ActivityLog>> GetByProjectIdAsync(long projectId, CancellationToken cancellationToken = default)
    {
       var specification = new GetActivityByProjectIdSpecification(projectId);
       return GetAsync(specification, cancellationToken);
    }

    public Task<IReadOnlyList<ActivityLog>> GetByTaskIdAsync(long taskId, CancellationToken cancellationToken = default)
    {
        var specification = new GetActivityByTaskIdSpecification(taskId);
        return GetAsync(specification, cancellationToken);
    }

    public Task<IReadOnlyList<ActivityLog>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        var specification = new GetActivityByUserIdSpecification(userId);
        return GetAsync(specification, cancellationToken);
    }
}