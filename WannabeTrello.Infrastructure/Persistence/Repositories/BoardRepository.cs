using Microsoft.EntityFrameworkCore;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Infrastructure.Persistence.Repositories;

public class BoardRepository(ApplicationDbContext dbContext) : Repository<Board>(dbContext), IBoardRepository
{
    public override async Task AddAsync(Board board) => await base.AddAsync(board);
    public override async Task<IEnumerable<Board>> GetAllAsync() => await base.GetAllAsync();
    public override async Task<Board?> GetByIdAsync(long id) => await base.GetByIdAsync(id);
    public async Task UpdateAsync(Board board) =>  base.Update(board); 
    public async Task DeleteAsync(long id)
    {
        var board = await GetByIdAsync(id);
        if (board != null)
        {
            base.Delete(board);
        }
    }

    public async Task<Board?> GetBoardWithDetailsAsync(long boardId)
    {
        return await _dbSet
            .Include(b => b.Columns.OrderBy(c => c.Order))
                .ThenInclude(c => c.Tasks.OrderBy(t => t.CreatedAt))
                .ThenInclude(t => t.Assignee)
            .Include(b => b.Columns)
                .ThenInclude(c => c.Tasks)
                .ThenInclude(t => t.Comments.OrderBy(cm => cm.CreatedAt))
            .Include(b => b.BoardMembers)
            .ThenInclude(bm => bm.User)
            .AsSplitQuery()
            .FirstOrDefaultAsync(b => b.Id == boardId);
    }

    public Task<List<Board>> GetBoardsByProjectIdAsync(long projectId, CancellationToken cancellationToken)
    {
        return _dbSet.Where(b => b.ProjectId == projectId).ToListAsync(cancellationToken);
    }
}