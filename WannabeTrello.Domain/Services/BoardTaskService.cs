using System.Reflection.Emit;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces;
using WannabeTrello.Domain.Interfaces.Repositories;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Domain.Services;

public class BoardTaskService(
    IBoardTaskRepository boardTaskRepository,
    IColumnRepository columnRepository,
    IBoardRepository boardRepository,
    IUserRepository userRepository,
    ICommentRepository commentRepository,
    IUnitOfWork unitOfWork) : IBoardTaskService
{
    public async Task<BoardTask> CreateTaskAsync(long columnId, string title, string? description,
        TaskPriority priority, DateTime dueDate, int position, long? assigneeId, long creatorUserId,
        CancellationToken cancellationToken)
    {
        var column = await columnRepository.GetByIdAsync(columnId);
        if (column == null) throw new NotFoundException(nameof(Column), columnId);


        var board = await boardRepository.GetBoardWithDetailsAsync(column.BoardId, cancellationToken);
        if (board == null || !board.BoardMembers.Any(bm => bm.UserId == creatorUserId && bm.Role == BoardRole.Editor))
        {
            throw new AccessDeniedException("You don't have permission to create tasks on this board.");
        }

        if (assigneeId.HasValue)
        {
            var assignee = await userRepository.GetByIdAsync(assigneeId.Value);
            if (assignee == null) throw new NotFoundException(nameof(User), assigneeId.Value);
        }

        var task = BoardTask.Create(title, description, priority, dueDate, position, columnId, assigneeId,
            creatorUserId);

        await boardTaskRepository.AddAsync(task, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return task;
    }

    public async Task<BoardTask?> GetTaskByIdAsync(long taskId, long userId, CancellationToken cancellationToken)
    {
        var task = await boardTaskRepository.GetTaskDetailsByIdAsync(taskId, cancellationToken)!;
        if (task == null)
            throw new NotFoundException(nameof(BoardTask), taskId);

        var userIsMemberOfBoard = task.Column.Board.BoardMembers.Any(bm => bm.UserId == userId);
        return !userIsMemberOfBoard
            ? throw new AccessDeniedException("You don't have permission to view this task.")
            : task;
    }

    public async Task<IReadOnlyList<BoardTask>> GetTasksByBoardIdAsync(long boardId, long userId, CancellationToken cancellationToken)
    {
        var board = await boardRepository.GetBoardWithDetailsAsync(boardId, cancellationToken);
        
        if (board == null) throw new NotFoundException(nameof(Board), boardId);
        
        if(board.BoardMembers.All(bm => bm.UserId != userId))
            throw new AccessDeniedException("You don't have permission to view this board.");
        
        return await boardTaskRepository.GetTasksByBoardIdAsync(boardId, cancellationToken);
    }

    public async Task UpdateTaskDetailsAsync(long taskId, string newTitle, string? newDescription,
        TaskPriority newPriority, DateTime newDueDate, long modifierUserId, CancellationToken cancellationToken)
    {
        var task = await boardTaskRepository.GetTaskDetailsByIdAsync(taskId, cancellationToken)!;
        if (task == null) 
            throw new NotFoundException(nameof(BoardTask), taskId);


        var board = task.Column.Board;
        if (!board.BoardMembers.Any(bm => bm.UserId == modifierUserId && bm.Role == BoardRole.Editor))
        {
            throw new AccessDeniedException("You don't have a permission to update this task.");
        }
        
        task.UpdateDetails(newTitle, newDescription, newPriority, newDueDate, modifierUserId);

        boardTaskRepository.Update(task);
        await unitOfWork.CompleteAsync(cancellationToken);
    }

    public async Task RestoreTaskAsync(long taskId, long modifierUserId, CancellationToken cancellationToken)
    {
        var task = await boardTaskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task == null) throw new NotFoundException(nameof(BoardTask), taskId);
        
        task.Restore(modifierUserId);
        
        boardTaskRepository.Update(task);
        await unitOfWork.CompleteAsync(cancellationToken);
    }

    public async Task MoveTaskAsync(long taskId, long newColumnId, long performingUserId, CancellationToken cancellationToken)
    {
        var task = await boardTaskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task == null) throw new NotFoundException(nameof(BoardTask), taskId);

        var oldColumnId = task.ColumnId;

        var newColumn = await columnRepository.GetByIdAsync(newColumnId, cancellationToken);
        if (newColumn == null) throw new NotFoundException(nameof(Column), newColumnId);


        var oldColumn = await columnRepository.GetByIdAsync(oldColumnId, cancellationToken);
        if (oldColumn == null) throw new NotFoundException(nameof(Column), oldColumnId);

        if (oldColumn.BoardId != newColumn.BoardId)
        {
            throw new InvalidOperationDomainException(
                "Cannot move tasks on this board task.");
        }

        var board = await boardRepository.GetBoardWithDetailsAsync(oldColumn.BoardId, cancellationToken);
        if (board == null ||
            !board.BoardMembers.Any(bm => bm.UserId == performingUserId && bm.Role == BoardRole.Editor))
        {
            throw new AccessDeniedException("You don't have permission to move this task.");
        }


        task.MoveToColumn(newColumn.Id, performingUserId);

        boardTaskRepository.Update(task);
        await unitOfWork.CompleteAsync(cancellationToken);
    }

    public async Task<long> AddCommentToTaskAsync(long taskId, long userId, string content)
    {
        var task = await boardTaskRepository.GetByIdAsync(taskId);
        if (task == null) throw new NotFoundException(nameof(BoardTask), taskId);

        var user = await userRepository.GetByIdAsync(userId);
        if (user == null) throw new NotFoundException(nameof(User), userId);

        // Provera autorizacije: Da li korisnik ima dozvolu za komentarisanje
        var column = await columnRepository.GetByIdAsync(task.ColumnId);
        if (column == null) throw new NotFoundException(nameof(Column), task.ColumnId);
        var board = await boardRepository.GetBoardWithDetailsAsync(column.BoardId);
        if (board == null ||
            !board.BoardMembers.Any(bm =>
                bm.UserId == userId && bm.Role != BoardRole.Viewer)) // Dozvoli samo urednicima
        {
            throw new AccessDeniedException("Nemate dozvolu za dodavanje komentara na ovaj zadatak.");
        }

        var comment = task.AddComment(content, userId);

        await commentRepository.AddAsync(comment);
        await unitOfWork.CompleteAsync();

        return comment.Id;
    }

    public async Task AssignTaskToUserAsync(long taskId, long? assigneeId, long performingUserId)
    {
        var task = await boardTaskRepository.GetByIdAsync(taskId);
        if (task == null) throw new NotFoundException(nameof(BoardTask), taskId);


        var column = await columnRepository.GetByIdAsync(task.ColumnId);
        if (column == null) throw new NotFoundException(nameof(Column), task.ColumnId);
        var board = await boardRepository.GetBoardWithDetailsAsync(column.BoardId);
        if (board == null ||
            !board.BoardMembers.Any(bm => bm.UserId == performingUserId && bm.Role == BoardRole.Editor))
        {
            throw new AccessDeniedException("Nemate dozvolu za dodeljivanje zadataka.");
        }

        User? assignee = null;
        if (assigneeId.HasValue)
        {
            assignee = await userRepository.GetByIdAsync(assigneeId.Value);
            if (assignee == null) throw new NotFoundException(nameof(User), assigneeId.Value);
        }


        task.AssignToUser(assigneeId!.Value, performingUserId);

        boardTaskRepository.Update(task);
        await unitOfWork.CompleteAsync();
    }

    public IQueryable<BoardTask> SearchTasks(long userId)
    {
       
        return boardTaskRepository.SearchTasks()
            .Where(t => t.Column.Board.BoardMembers.Any(bm => bm.UserId == userId));
    }

    public async Task ArchiveTaskAsync(long taskId, long modifierUserId, CancellationToken cancellationToken)
    {
        var task = await boardTaskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task == null) throw new NotFoundException(nameof(BoardTask), taskId);
        
        task.Archive(modifierUserId);
        
        boardTaskRepository.Update(task);
        await unitOfWork.CompleteAsync(cancellationToken);
    }
}