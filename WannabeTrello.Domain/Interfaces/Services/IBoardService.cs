using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Interfaces.Services;

public interface IBoardService
{
    Task<Board> CreateBoardAsync(long projectId, string name, string? description, long creatorUserId, CancellationToken cancellationToken);
    Task<List<Board>> GetBoardByProjectIdAsync(long projectId, long userId, CancellationToken cancellationToken);
}