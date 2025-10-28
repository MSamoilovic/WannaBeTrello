using Microsoft.EntityFrameworkCore;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Infrastructure.Persistence.Repositories;

public class BoardTaskRepository(ApplicationDbContext dbContext)
    : Repository<BoardTask>(dbContext), IBoardTaskRepository
{
    public async Task<IEnumerable<BoardTask>> GetTasksByColumnIdAsync(long columnId)
    {
        return await _dbSet
            .Where(t => t.ColumnId == columnId)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<BoardTask?> GetTaskDetailsByIdAsync(long id, CancellationToken cancellationToken)
    {
        return await _dbSet.Include(t => t.Assignee)
            .Include(t => t.Comments)
            .ThenInclude(c => c.User)
            .Include(t => t.Column)
            .ThenInclude(c => c.Board)
            .ThenInclude(b => b.BoardMembers)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public override async Task AddAsync(BoardTask task) => await base.AddAsync(task);
    public override async Task<BoardTask?> GetByIdAsync(long id) => await base.GetByIdAsync(id);

    public async Task UpdateAsync(BoardTask task) => base.Update(task);

    public async Task DeleteAsync(long id)
    {
        var task = await GetByIdAsync(id);
        if (task != null) base.Delete(task);
    }

    public IQueryable<BoardTask> SearchTask()
    {
        return _dbContext.Tasks
            .Include(t => t.Column)
            .Include(t => t.Assignee)
            .Include(t => t.Comments)
            .ThenInclude(c => c.User)
            .AsSplitQuery()
            .AsQueryable();
    }

    public override async Task<IEnumerable<BoardTask>> GetAllAsync() => await base.GetAllAsync();
}