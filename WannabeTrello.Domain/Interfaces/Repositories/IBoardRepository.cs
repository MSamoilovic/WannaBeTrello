using WannabeTrello.Domain.Entities;


namespace WannabeTrello.Domain.Interfaces.Repositories;

public interface IBoardRepository
{
    Task<Board?> GetByIdAsync(long id);
    Task<IEnumerable<Board>> GetAllAsync();
    Task AddAsync(Board board);
    Task UpdateAsync(Board board);
    Task DeleteAsync(long id);
    Task<Board?> GetBoardWithDetailsAsync(long boardId, CancellationToken cancellationToken=default);
    
    Task<List<Board>> GetBoardsByProjectIdAsync(long projectId, CancellationToken  cancellationToken);
}