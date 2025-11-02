using Microsoft.EntityFrameworkCore;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Infrastructure.Persistence.Repositories;

public class BoardRepository: Repository<Board>, IBoardRepository
{
    public BoardRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Board?> GetBoardWithDetailsAsync(long boardId, CancellationToken cancellationToken = default)
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
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(b => b.Id == boardId, cancellationToken);
    }

    public async Task<Board?> GetBoardWithColumnsAsync(long boardId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(b => b.Columns)
            .FirstOrDefaultAsync(b => b.Id == boardId, cancellationToken);
    }

    public async Task<IReadOnlyList<Board>> GetBoardsByProjectIdAsync(long projectId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(b => b.ProjectId == projectId)
            .OrderBy(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async  Task<bool> IsBoardMemberAsync(long boardId, long userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.BoardMembers
            .AnyAsync(bm => bm.BoardId == boardId && bm.UserId == userId, cancellationToken);
    }
}