using WannabeTrello.Domain.Entities;


namespace WannabeTrello.Domain.Interfaces.Repositories;

public interface IBoardRepository: IRepository<Board>
{
    Task<Board?> GetBoardWithDetailsAsync(long boardId, CancellationToken cancellationToken = default);
    Task<Board?> GetBoardWithColumnsAsync(long boardId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Board>> GetBoardsByProjectIdAsync(long projectId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Column>> GetColumnsByBoardIdAsync(long boardId, CancellationToken cancellationToken = default);
    Task<bool> IsBoardMemberAsync(long boardId, long userId, CancellationToken cancellationToken = default);
}