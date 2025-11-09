using WannabeTrello.Domain.Events.Comment_Events;
using WannabeTrello.Domain.Exceptions;

namespace WannabeTrello.Domain.Entities;

public class Comment: AuditableEntity
{
    public string? Content { get; private set; } 
    public long TaskId { get; private set; }
    public BoardTask Task { get; private set; } = null!; 
    public long UserId { get; private set; } 
    public User User { get; private set; } = null!; 
    public bool IsDeleted { get; private set; }
    public bool IsEdited { get; private set; }
    public DateTime? EditedAt { get; private set; }
    
    private Comment() {}

    public static Comment Create(long taskId, string content, long userId)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new BusinessRuleValidationException("Comment content cannot be empty.");
        if (content.Length > 300)
        {
            throw new BusinessRuleValidationException("Comment content cannot be longer than 300 characters.");
        }
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

    public void UpdateContent(string? newContent, long modifyingUserId)
    {
        if(string.IsNullOrEmpty(newContent))
            throw new BusinessRuleValidationException("Comment content cannot be empty.");
        
        if (newContent.Length > 300)
            throw new BusinessRuleValidationException("Comment content cannot be longer than 300 characters.");
        
        if(IsDeleted)
            throw new BusinessRuleValidationException("Comment is deleted.");
        
        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();
        
        
        oldValues[nameof(Content)] = Content;
        Content = newContent;
        newValues[nameof(Content)] = newContent;
        
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifyingUserId;
        IsEdited = true;
        EditedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CommentUpdatedEvent(Id, TaskId, oldValues, newValues, modifyingUserId));
    }

    public void Delete(long modifierUserId)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifierUserId;
        
        AddDomainEvent(new CommentDeletedEvent(Id, TaskId, modifierUserId));
    }

    public void Restore(long modifierUserId)
    {
        if (!IsDeleted) return;
        IsDeleted = false;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifierUserId;
        
        AddDomainEvent(new CommentRestoredEvent(Id, TaskId, modifierUserId));
    }
}