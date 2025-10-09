using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Events.TaskEvents;
using WannabeTrello.Domain.Exceptions;

namespace WannabeTrello.Domain.Entities;

public class BoardTask: AuditableEntity
{
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int Position { get; private set; }
    public TaskPriority Priority { get; private set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; private set; }
    
    public long ColumnId { get; private set; }
    public Column Column { get; private set; } = null!;
    
    public long? AssigneeId { get; private set; }
    public User? Assignee { get; private set; }
    
    public ICollection<Comment> Comments { get; private set; } = new List<Comment>();
    
    private BoardTask() { }
    
    public static BoardTask Create(string title, string? description, TaskPriority priority, DateTime dueDate, int position, long columnId, long? assigneeId, long creatorUserId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Naslov zadatka ne može biti prazan.", nameof(title));
        
        if (columnId < 1)
            throw new ArgumentException("Zadatak mora pripadati koloni.", nameof(columnId));

        var task = new BoardTask
        {
            Title = title,
            Description = description,
            Priority = priority,
            DueDate = dueDate,
            Position = position,
            ColumnId = columnId,
            AssigneeId = assigneeId
        };

        
        task.AddDomainEvent(new TaskCreatedEvent(task.Id, task.Title, creatorUserId)); 

        return task;
    }
    
    public void UpdateDetails(
        string newTitle, 
        string? newDescription, 
        TaskPriority newPriority, 
        DateTime newDueDate, 
        long modifierUserId
    )
    {
        if (string.IsNullOrWhiteSpace(newTitle))
            throw new ArgumentException("Naslov taska ne može biti prazan.", nameof(newTitle));
        
        if (string.IsNullOrWhiteSpace(newTitle))
            throw new BusinessRuleValidationException("Task title cannot be empty.");
        
        var changed = false;
        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();

        if (Title != newTitle) 
        {
            oldValues[nameof(Title)] = Title;
            newValues[nameof(Title)] = newTitle;
            Title = newTitle; 
            changed = true;
        }
        if (Description != newDescription) 
        {
            oldValues[nameof(Description)] = Description;
            newValues[nameof(Description)] = newDescription;
            Description = newDescription; 
            changed = true;
        }
        if (Priority != newPriority) 
        {
            oldValues[nameof(Priority)] = Priority;
            newValues[nameof(Priority)] = newPriority;
            Priority = newPriority; 
            changed = true;
        }
        if (DueDate != newDueDate) 
        {
            oldValues[nameof(DueDate)] = DueDate;
            newValues[nameof(DueDate)] = newDueDate;
            DueDate = newDueDate; 
            changed = true;
        }

        if (!changed) return;
        
        AddDomainEvent(new TaskUpdatedEvent(Id, modifierUserId)); // Aktiviraj događaj za ažuriranje zadatka
    }
    
    public void MoveToColumn(long newColumnId, long performingUserId)
    {
        if (ColumnId == newColumnId) return;

        var originalColumnId = ColumnId;
        ColumnId = newColumnId;
        
        AddDomainEvent(new TaskMovedEvent(Id, originalColumnId, newColumnId, performingUserId));
    }
    
    public Comment AddComment(string content, long userId)
    {
        
        //TODO: Change Comment to use Create method
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Sadržaj komentara ne može biti prazan.", nameof(content));
        
        if (userId < 1)
            throw new ArgumentException("Komentar mora imati autora.", nameof(userId));

        var comment = new Comment
        {
            TaskId = this.Id,
            UserId = userId,
            Content = content,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
        this.Comments.Add(comment);

        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = userId;

        //Napravi TaskCommentedEvent
        return comment;
    }
    
    public void AssignToUser(long? newAssigneeId, long performingUserId)
    {
        if (AssigneeId == newAssigneeId) return;

        var oldAssigneeId = AssigneeId;
        AssigneeId = newAssigneeId;
        
        // Audit propertiji se postavljaju u DbContext-u.
        AddDomainEvent(new TaskAssignedEvent(Id, oldAssigneeId, newAssigneeId, performingUserId));
    }

}