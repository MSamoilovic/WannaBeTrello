using Microsoft.EntityFrameworkCore;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Infrastructure.Persistence.Repositories;

public class ColumnRepository(ApplicationDbContext dbContext) : Repository<Column>(dbContext), IColumnRepository
{
    public async Task<IEnumerable<Column>> GetColumnsByBoardIdAsync(long boardId)
    {
        return await _dbSet
            .Where(c => c.BoardId == boardId)
            .OrderBy(c => c.Order) 
            .ToListAsync();
    }

   
    public async Task AddAsync(Column column) => await base.AddAsync(column);
    public async Task<Column?> GetByIdAsync(long id) => await base.GetByIdAsync(id);
    
    public async Task UpdateAsync(Column column) => base.Update(column);
    
    public async Task DeleteAsync(long id)
    {
        var column = await GetByIdAsync(id);
        if (column != null) base.Delete(column);
    }
}