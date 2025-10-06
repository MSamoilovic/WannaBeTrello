using Microsoft.EntityFrameworkCore;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Infrastructure.Persistence.Repositories;

public class ColumnRepository(ApplicationDbContext dbContext) : Repository<Column>(dbContext), IColumnRepository
{
    public Task<Column?> GetByIdWithBoardsAndMembersAsync(long id, CancellationToken cancellationToken)
    {
       return dbContext.Columns
           .Include(x => x.Board)
              .ThenInclude(b => b.BoardMembers)
           .AsNoTracking()
           .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task AddAsync(Column column) => await base.AddAsync(column);
    public async Task<Column?> GetByIdAsync(long id) => await base.GetByIdAsync(id);
    
    public async Task UpdateAsync(Column column) =>   base.Update(column);
    
    public async Task DeleteAsync(long id)
    {
        var column = await GetByIdAsync(id);
        if (column != null) base.Delete(column);
    }
}