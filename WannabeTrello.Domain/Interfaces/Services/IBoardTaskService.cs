using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Domain.Interfaces.Services;

public interface IBoardTaskService
{
    public Task<BoardTask> CreateTaskAsync(long columnId, string title, string? description, TaskPriority priority, DateTime dueDate, int position, long? assigneeId, long creatorUserId, CancellationToken cancellationToken); 
    public Task<BoardTask?> GetTaskByIdAsync(long taskId, long userId, CancellationToken cancellationToken);
    public Task<List<BoardTask>> GetTasksByBoardIdAsync(long boardId, long userId, CancellationToken cancellationToken);
    public Task UpdateTaskDetailsAsync(long taskId, string newTitle, string? newDescription, TaskPriority newPriority, DateTime newDueDate, long modifierUserId, CancellationToken cancellationToken);
}