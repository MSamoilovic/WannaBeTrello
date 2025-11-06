using WannabeTrello.Domain.Exceptions;

namespace WannabeTrello.Domain.Entities;

public class Comment: AuditableEntity
{
    public string Content { get; private set; } = null!;
    public long TaskId { get; private set; }
    public BoardTask Task { get; private set; } = null!; 
    public long UserId { get; private set; } 
    public User User { get; private set; } = null!; 
    
    private Comment() {}

    public static Comment Create(long taskId, string content, long userId)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new BusinessRuleValidationException("Comment content cannot be empty.");
        if (taskId < 1)
            throw new BusinessRuleValidationException("TaskId must be a positive number.");
        if (userId < 1)
            throw new BusinessRuleValidationException("Comment must have a valid author.");

        return new Comment
        {
            TaskId = taskId,
            UserId = userId,
            Content = content,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
    }
    
    //Izmeniti domain model da ima svoje aktivnosti
}