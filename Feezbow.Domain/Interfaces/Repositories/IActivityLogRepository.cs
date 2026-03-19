using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Interfaces.Repositories;

public interface IActivityLogRepository: IRepository<ActivityLog>
{
    Task<IReadOnlyList<ActivityLog>> GetByTaskIdAsync(long taskId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ActivityLog>> GetByProjectIdAsync(long projectId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ActivityLog>> GetByBoardIdAsync(long boardId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ActivityLog>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default);
}