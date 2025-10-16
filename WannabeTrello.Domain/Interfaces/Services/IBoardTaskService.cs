using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Domain.Interfaces.Services;

public interface IBoardTaskService
{
    public Task<BoardTask> CreateTaskAsync(long columnId, string title, string? description, TaskPriority priority, DateTime dueDate, int position, long? assigneeId, long creatorUserId, CancellationToken cancellationToken); 
}