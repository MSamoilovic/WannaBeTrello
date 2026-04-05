using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Domain.Services;

public class ColumnService(
    IColumnRepository columnRepository,
    IBoardRepository boardRepository,
    IUnitOfWork unitOfWork) : IColumnService
{
    public async Task<Column> CreateColumnAsync(long boardId, string? name, int order, long userId,
        CancellationToken cancellationToken)
    {
        var board = await boardRepository.GetBoardWithDetailsAsync(boardId, cancellationToken);
        if (board == null)
            throw new NotFoundException(nameof(Board), boardId);

        if (board.IsArchived)
            throw new InvalidOperationException($"Board {boardId} is already archived");

        var boardMember = board.BoardMembers.FirstOrDefault(x => x.UserId == userId);
        if (boardMember is not { Role: BoardRole.Admin })
            throw new AccessDeniedException("Only Board Admin can create a Column");
        
        var existingColumns = await columnRepository.GetColumnsByBoardIdAsync(boardId, cancellationToken);
        if (existingColumns.Any(c => c.Order == order))
            order = existingColumns.Max(c => c.Order) + 1;

        var column = new Column(name, boardId, order, userId);

        await columnRepository.AddAsync(column);
        await unitOfWork.CompleteAsync(cancellationToken);

        return column;
    }
    
    public async Task<Column> GetColumnByIdAsync(long columnId, long userId, CancellationToken cancellationToken)
    {
        var column = await columnRepository.GetColumnDetailsByIdAsync(columnId, cancellationToken);
        return column ?? throw new NotFoundException(nameof(Column), columnId);
    }

    public async Task<Column> UpdateColumnAsync(long columnId, string? newName, int? wipLimit, long userId, CancellationToken cancellationToken)
    {
        var column = await columnRepository.GetColumnWithBoardAndMembersAsync(columnId, cancellationToken);
        if (column == null)
            throw new NotFoundException(nameof(Column), columnId);
        
        var member = column.Board.BoardMembers.FirstOrDefault(m => m.UserId == userId);
        if (member is not { Role: BoardRole.Admin })
            throw new AccessDeniedException("Only a Board Admin can update a column's details.");
        
        if (newName != null)
        {
            column.ChangeName(newName, userId);
        }
        
        if (wipLimit.HasValue)
        {
            column.SetWipLimit(wipLimit.Value,  userId);
        }
        
        columnRepository.Update(column);
        await unitOfWork.CompleteAsync(cancellationToken);
        
        return column;
    }

    public async Task<long> DeleteColumnAsync(long columnId, long userId, CancellationToken cancellationToken)
    {
        var column  = await columnRepository.GetColumnWithBoardAndMembersAsync(columnId, cancellationToken);
        if (column == null)
            throw new NotFoundException(nameof(Column), columnId);
        
        var member = column.Board.BoardMembers.FirstOrDefault(m => m.UserId == userId);
        if (member is not { Role: BoardRole.Admin })
            throw new AccessDeniedException("Only a Board Admin can delete a column.");
        
        column.DeleteColumn(userId);
        
        // Soft delete by marking as deleted, then save changes
        columnRepository.Update(column);
        await unitOfWork.CompleteAsync(cancellationToken);
        
        return column.Id;
    }

    
}