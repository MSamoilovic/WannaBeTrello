using Microsoft.EntityFrameworkCore;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Interfaces.Repositories;
using Feezbow.Domain.Specifications.TaskSpecifications;

namespace Feezbow.Infrastructure.Persistence.Repositories;

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

    public async Task<long> GetBoardIdByTaskIdAsync(long taskId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<BoardTask>()
            .Where(t => t.Id == taskId)
            .Select(t => t.Column.BoardId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<BoardTask?> GetTaskWithLabelsAsync(long taskId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.TaskLabels)
                .ThenInclude(tl => tl.Label)
            .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);
    }
}