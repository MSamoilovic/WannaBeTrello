using Microsoft.EntityFrameworkCore;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces.Repositories;
using WannabeTrello.Domain.Specifications.TaskSpecifications;

namespace WannabeTrello.Infrastructure.Persistence.Repositories;

public class BoardTaskRepository(ApplicationDbContext dbContext)
    : Repository<BoardTask>(dbContext),IBoardTaskRepository
{
    public async Task<IReadOnlyList<BoardTask>> GetTasksByBoardIdAsync(long boardId, CancellationToken cancellationToken)
    {
        var specification = new TasksByBoardIdSpecification(boardId);
        return await GetAsync(specification, cancellationToken);
    }

    public async Task<BoardTask>? GetTaskDetailsByIdAsync(long id, CancellationToken cancellationToken)
    {
        var specification = new TaskDetailsByIdSpecification(id);
        return await GetSingleAsync(specification, cancellationToken);
    }

    public IQueryable<BoardTask> SearchTasks()
    {
        var specification = new SearchTasksSpecification();
        return ApplySpecification(specification);
    }
}