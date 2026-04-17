using Microsoft.EntityFrameworkCore;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Infrastructure.Persistence.Repositories;

public class ShoppingListRepository(ApplicationDbContext dbContext) : IShoppingListRepository
{
    private readonly DbSet<ShoppingList> _dbSet = dbContext.Set<ShoppingList>();

    public async Task<ShoppingList?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(l => l.Project)
            .ThenInclude(p => p.ProjectMembers)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<ShoppingList?> GetByIdWithItemsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(l => l.Project)
            .ThenInclude(p => p.ProjectMembers)
            .Include(l => l.Items)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ShoppingList>> GetByProjectAsync(long projectId, bool includeArchived, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(l => l.Items)
            .Where(l => l.ProjectId == projectId);

        if (!includeArchived)
            query = query.Where(l => !l.IsArchived);

        return await query
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ShoppingList list, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(list, cancellationToken);
    }

    public void Remove(ShoppingList list)
    {
        _dbSet.Remove(list);
    }
}
