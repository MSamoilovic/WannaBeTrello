using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Interfaces.Repositories;

public interface IColumnRepository
{
    Task<Column?> GetByIdAsync(long id);
    Task<IEnumerable<Column>> GetColumnsByBoardIdAsync(long boardId);
    Task AddAsync(Column column);
    Task UpdateAsync(Column column);
    Task DeleteAsync(long id);
}