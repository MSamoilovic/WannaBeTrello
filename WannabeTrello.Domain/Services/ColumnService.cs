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
    IUnitOfWork unitOfWork) : IColumnService
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
    
    public async Task<Column> GetColumnByIdAsync(long boardId, long userId, CancellationToken cancellationToken)
    {
        var column = await columnRepository.GetByIdAsync(boardId);
        return column ?? throw new NotFoundException(nameof(Column), boardId);
    }

    public async Task<Column> UpdateColumnAsync(long columnId, string? newName, int? wipLimit, long userId, CancellationToken cancellationToken)
    {
        var column = await columnRepository.GetByIdWithBoardsAndMembersAsync(columnId, cancellationToken);
        if (column == null)
            throw new NotFoundException(nameof(Column), columnId);
        
        var member = column.Board.BoardMembers.FirstOrDefault(m => m.UserId == userId);
        if (member is not { Role: BoardRole.Admin })
            throw new AccessDeniedException("Only a Board Admin can update a column's details.");
        
        if (newName != null)
        {
            column.ChangeName(newName);
        }
        
        if (wipLimit.HasValue)
        {
            column.SetWipLimit(wipLimit.Value);
        }
        
        await unitOfWork.CompleteAsync(cancellationToken);
        
        return column;
    }

    public async Task<long> DeleteColumnAsync(long columnId, long? userId, CancellationToken cancellationToken)
    {
        var column  = await columnRepository.GetByIdWithBoardsAndMembersAsync(columnId, cancellationToken);
        if (column == null)
            throw new NotFoundException(nameof(Column), columnId);
        
        var member = column.Board.BoardMembers.FirstOrDefault(m => m.UserId == userId);
        if (member is not { Role: BoardRole.Admin })
            throw new AccessDeniedException("Only a Board Admin can delete a column.");
        
        await columnRepository.DeleteAsync(column.Id);
        await unitOfWork.CompleteAsync(cancellationToken);
        
        return column.Id;
    }

    public async Task ReorderColumnsAsync(long boardId, Dictionary<long, int> columnOrders, long userId, CancellationToken cancellationToken)
    {
        var board = await boardRepository.GetBoardWithDetailsAsync(boardId, cancellationToken);
        if (board == null)
            throw new NotFoundException(nameof(Board), boardId);

        var member = board.BoardMembers.FirstOrDefault(m => m.UserId == userId);
        if (member is not { Role: BoardRole.Admin })
            throw new AccessDeniedException("Only a Board Admin can reorder columns.");

        foreach (var orderInfo in columnOrders)
        {
            var columnToUpdate = board.Columns.FirstOrDefault(c => c.Id == orderInfo.Key);
            columnToUpdate?.ChangeOrder(orderInfo.Value);
        }

        await unitOfWork.CompleteAsync(cancellationToken); 
    }
}