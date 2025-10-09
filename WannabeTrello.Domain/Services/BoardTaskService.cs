using System.Reflection.Emit;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Domain.Services;

public class BoardTaskService(
    IBoardTaskRepository boardTaskRepository,
    IColumnRepository columnRepository,
    IBoardRepository boardRepository,
    IUserRepository userRepository,
    ICommentRepository commentRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<BoardTask> CreateTaskAsync(long columnId, string title, string? description, TaskPriority priority, DateTime dueDate, int position, long? assigneeId, long creatorUserId)
        {
            var column = await columnRepository.GetByIdAsync(columnId);
            if (column == null) throw new NotFoundException(nameof(Column), columnId);

            
            var board = await boardRepository.GetBoardWithDetailsAsync(column.BoardId);
            if (board == null || !board.BoardMembers.Any(bm => bm.UserId == creatorUserId && bm.Role == BoardRole.Editor))
            {
                throw new AccessDeniedException("Nemate dozvolu za kreiranje zadataka na ovoj tabli.");
            }
            
            User? assignee = null;
            
            if (assigneeId.HasValue)
            {
                assignee = await userRepository.GetByIdAsync(assigneeId.Value);
                if (assignee == null) throw new NotFoundException(nameof(User), assigneeId.Value);
            }
            
            var task = BoardTask.Create(title, description, priority, dueDate, position, columnId, assigneeId, creatorUserId);

            await boardTaskRepository.AddAsync(task);
            await unitOfWork.CompleteAsync();

            

            return task;
        }

        public async Task UpdateTaskDetailsAsync(long taskId, string newTitle, string? newDescription, TaskPriority newPriority, DateTime newDueDate, long modifierUserId)
        {
            var task = await boardTaskRepository.GetByIdAsync(taskId);
            if (task == null) throw new NotFoundException(nameof(BoardTask), taskId);

            
            var column = await columnRepository.GetByIdAsync(task.ColumnId);
            if (column == null) throw new NotFoundException(nameof(Column), task.ColumnId);
            
            var board = await boardRepository.GetBoardWithDetailsAsync(column.BoardId);
            if (board == null || !board.BoardMembers.Any(bm => bm.UserId == modifierUserId && bm.Role == BoardRole.Editor))
            {
                throw new AccessDeniedException("Nemate dozvolu za ažuriranje ovog zadatka.");
            }

            // Delegiranje logike ažuriranja entitetu
            task.UpdateDetails(newTitle, newDescription, newPriority, newDueDate, modifierUserId);

            await boardTaskRepository.UpdateAsync(task);
            await unitOfWork.CompleteAsync();
        }

        public async Task MoveTaskAsync(long taskId, long newColumnId, long performingUserId)
        {
            
            var task = await boardTaskRepository.GetByIdAsync(taskId); 
            if (task == null) throw new NotFoundException(nameof(BoardTask), taskId);

            var oldColumnId = task.ColumnId;

            var newColumn = await columnRepository.GetByIdAsync(newColumnId);
            if (newColumn == null) throw new NotFoundException(nameof(Column), newColumnId);

            
            var oldColumn = await columnRepository.GetByIdAsync(oldColumnId);
            if (oldColumn == null) throw new NotFoundException(nameof(Column), oldColumnId);

            if (oldColumn.BoardId != newColumn.BoardId)
            {
                throw new InvalidOperationDomainException("Ne možete premestiti zadatak između različitih tabli putem ove operacije.");
            }

            var board = await boardRepository.GetBoardWithDetailsAsync(oldColumn.BoardId);
            if (board == null || !board.BoardMembers.Any(bm => bm.UserId == performingUserId && bm.Role == BoardRole.Editor))
            {
                throw new AccessDeniedException("Nemate dozvolu za premeštanje zadataka na ovoj tabli.");
            }

           
            task.MoveToColumn(newColumn.Id, performingUserId); 

            await boardTaskRepository.UpdateAsync(task);
            await unitOfWork.CompleteAsync();
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
            if (board == null || !board.BoardMembers.Any(bm => bm.UserId == userId && bm.Role != BoardRole.Viewer)) // Dozvoli samo urednicima
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
            if (board == null || !board.BoardMembers.Any(bm => bm.UserId == performingUserId && bm.Role == BoardRole.Editor))
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

            await boardTaskRepository.UpdateAsync(task);
            await unitOfWork.CompleteAsync();
        }

      
    }