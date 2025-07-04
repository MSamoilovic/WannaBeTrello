using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Interfaces.Repositories;

public interface IBoardTaskRepository
{
    Task<BoardTask?> GetByIdAsync(long id);
    Task<IEnumerable<BoardTask>> GetTasksByColumnIdAsync(long columnId);
    Task AddAsync(BoardTask? task);
    Task UpdateAsync(BoardTask? task);
    Task DeleteAsync(long id);
    IQueryable<BoardTask> SearchTask();
    Task<IEnumerable<BoardTask>> GetAllAsync();
}