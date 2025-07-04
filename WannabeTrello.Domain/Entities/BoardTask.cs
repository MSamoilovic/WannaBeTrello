using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Events.TaskEvents;
using WannabeTrello.Domain.Exceptions;

namespace WannabeTrello.Domain.Entities;

public class BoardTask: AuditableEntity
{
    public long Id { get; private set; }
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
            AssigneeId = assigneeId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = creatorUserId
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
        
        var changed = false;
        
        if (Title != newTitle) { Title = newTitle; changed = true; }
        if (Description != newDescription) { Description = newDescription; changed = true; }
        if (Priority != newPriority) { Priority = newPriority; changed = true; }
        if (DueDate != newDueDate) { DueDate = newDueDate; changed = true; }

        if (!changed) return;
        
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifierUserId;
        AddDomainEvent(new TaskUpdatedEvent(Id, modifierUserId)); // Aktiviraj događaj za ažuriranje zadatka
    }
    
    public void Move(Column newColumn, long performingUserId)
    {
        if (newColumn == null)
            throw new ArgumentNullException(nameof(newColumn), "Nova kolona ne može biti null.");

       
        if (Column != null && newColumn.BoardId != Column.BoardId)
        {
            throw new InvalidOperationDomainException(
                $"Task '{Title}' ne može biti premešten u kolonu ('{newColumn.Name}') na drugoj tabli.");
        }

        if (ColumnId == newColumn.Id)
        {
            // Task je već u ovoj koloni, nema potrebe za izmenom
            return;
        }

        var originalColumnId = ColumnId;
        ColumnId = newColumn.Id;
        Column = newColumn; 
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = performingUserId;

        // Dodat Domenski Event
        AddDomainEvent(new TaskMovedEvent(Id, originalColumnId, newColumn.Id, performingUserId));
    }
    
    public Comment AddComment(string content, long userId)
    {
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
    
    public void AssignToUser(User? assignee, long performingUserId)
    {
        var oldAssigneeId = AssigneeId;

        if (assignee == null)
        {
            AssigneeId = null;
            Assignee = null;
        }
        else
        {
            AssigneeId = assignee.Id;
            Assignee = assignee;
        }

        if (oldAssigneeId == AssigneeId) return; 
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = performingUserId;
    }

}