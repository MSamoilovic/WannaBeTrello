using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces;
using WannabeTrello.Domain.Interfaces.Repositories;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Domain.Services;

public class ColumnService(
    IColumnRepository columnRepository,
    IBoardRepository boardRepository,
    IUnitOfWork unitOfWork,
    IUserRepository userRepository) : IColumnService
{
    public async Task<Column> CreateColumnAsync(long boardId, string? name, int order, long userId,
        CancellationToken cancellationToken)
    {
        var board = await boardRepository.GetByIdAsync(boardId);
        if (board == null)
            throw new NotFoundException(nameof(Board), boardId);

        if (board.IsArchived)
            throw new InvalidOperationException($"Board {boardId} is already archived");

        var boardMember = board.BoardMembers.FirstOrDefault(x => x.BoardId == boardId);
        if (boardMember is not { Role: BoardRole.Admin })
            throw new AccessDeniedException("Only Board Admin can create a Column");
        
        //TODO: implement change order logic in case there is already a column with the same order
        
        var column = new Column(name, boardId, order, userId);

        await columnRepository.AddAsync(column);
        await unitOfWork.CompleteAsync(cancellationToken);

        return column;
    }

    public Task<List<Column>> GetColumnsByBoardIdAsync(long boardId, long userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Column> GetColumnByIdAsync(long boardId, long userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}