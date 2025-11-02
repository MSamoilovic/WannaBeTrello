using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Interfaces.Repositories;

public interface IBoardTaskRepository: IRepository<BoardTask>
{
    Task<IReadOnlyList<BoardTask>> GetTasksByBoardIdAsync(long boardId, CancellationToken cancellationToken);
    Task<BoardTask>? GetTaskDetailsByIdAsync(long id, CancellationToken cancellationToken);
    IQueryable<BoardTask> SearchTasks();
}