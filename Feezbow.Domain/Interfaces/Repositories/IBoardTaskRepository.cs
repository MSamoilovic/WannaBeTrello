using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Interfaces.Repositories;

public interface IBoardTaskRepository: IRepository<BoardTask>
{
    Task<IReadOnlyList<BoardTask>> GetTasksByBoardIdAsync(long boardId, CancellationToken cancellationToken);
    Task<BoardTask?> GetTaskDetailsByIdAsync(long id, CancellationToken cancellationToken);
    IQueryable<BoardTask> SearchTasks();
    Task<long> GetBoardIdByTaskIdAsync(long taskId, CancellationToken cancellationToken = default);
    Task<BoardTask?> GetTaskWithLabelsAsync(long taskId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BoardTask>> GetTasksDueSoonAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
}