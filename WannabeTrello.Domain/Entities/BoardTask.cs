using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Events.TaskEvents;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.ValueObjects;

namespace WannabeTrello.Domain.Entities;

public class BoardTask : AuditableEntity
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

    public bool IsArchived { get; private set; }

    public ICollection<Comment> Comments { get; private set; } = new List<Comment>();

    private readonly List<Activity> _activities = [];
    public IReadOnlyCollection<Activity> Activities => _activities.AsReadOnly();

    private BoardTask()
    {
    }

    public static BoardTask Create(string title, string? description, TaskPriority priority, DateTime? dueDate,
        int position, long columnId, long? assigneeId, long creatorUserId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new BusinessRuleValidationException("Task title cannot be empty.");
        if (title.Length > 200)
            throw new BusinessRuleValidationException("Task title cannot exceed 200 characters.");

        if (columnId < 1)
            throw new BusinessRuleValidationException("ColumnId must be a positive number.");
        if (position < 0)
            throw new BusinessRuleValidationException("Position cannot be negative.");
        if (dueDate.HasValue && dueDate.Value < DateTime.UtcNow.Date)
            throw new BusinessRuleValidationException("Due date must be today or in the future.");

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

        task.AddDomainEvent(new TaskCreatedEvent(task.Id, task.Title, creatorUserId, assigneeId));

        return task;
    }

    public void UpdateDetails(
        string newTitle,
        string? newDescription,
        TaskPriority newPriority,
        DateTime? newDueDate,
        long modifierUserId
    )
    {
        if (string.IsNullOrWhiteSpace(newTitle))
            throw new BusinessRuleValidationException("Task title cannot be empty.");
        if (newTitle.Length > 200)
            throw new BusinessRuleValidationException("Task title cannot exceed 200 characters.");
        if (newDueDate.HasValue && newDueDate.Value < DateTime.UtcNow.Date)
            throw new BusinessRuleValidationException("Due date must be today or in the future.");

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

        AddDomainEvent(new TaskUpdatedEvent(Id, Title, modifierUserId, oldValues, newValues));
    }

    public void MoveToColumn(long newColumnId, long performingUserId)
    {
        if (ColumnId == newColumnId) return;
        if (newColumnId < 1)
            throw new BusinessRuleValidationException("New ColumnId must be a positive number.");

        var originalColumnId = ColumnId;
        ColumnId = newColumnId;

        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = performingUserId;

        AddDomainEvent(new TaskMovedEvent(Id, originalColumnId, newColumnId, performingUserId));
    }

    public Comment AddComment(string content, long userId)
    {
        var comment = Comment.Create(Id, content, userId);
        this.Comments.Add(comment);

        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = userId;

        if (Column == null)
            throw new BusinessRuleValidationException("Column must be loaded to comment on a task.");
        AddDomainEvent(new TaskCommentedEvent(Id, comment.Id, userId, Column.BoardId));
        return comment;
    }

    public void AssignToUser(long? newAssigneeId, long performingUserId)
    {
        if (AssigneeId == newAssigneeId) return;

        var oldAssigneeId = AssigneeId;
        AssigneeId = newAssigneeId;

        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = performingUserId;

        AddDomainEvent(new TaskAssignedEvent(Id, oldAssigneeId, newAssigneeId, performingUserId));
    }

    public void SetPosition(int newPosition, long modifierUserId)
    {
        if (newPosition < 0)
            throw new BusinessRuleValidationException("Position cannot be negative.");
        if (newPosition == Position) return;
        var old = Position;
        Position = newPosition;
        AddDomainEvent(new TaskUpdatedEvent(Id, Title, modifierUserId,
            new Dictionary<string, object?> { { nameof(Position), old } },
            new Dictionary<string, object?> { { nameof(Position), newPosition } }
        ));
    }

    public void Archive(long modifierUserId)
    {
        if (IsArchived) return;
        IsArchived = true;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifierUserId;
        AddDomainEvent(new TaskUpdatedEvent(Id, Title, modifierUserId,
            new Dictionary<string, object?> { { nameof(IsArchived), false } },
            new Dictionary<string, object?> { { nameof(IsArchived), true } }
        ));
    }

    public void Restore(long modifierUserId)
    {
        if (!IsArchived) return;
        IsArchived = false;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifierUserId;
        AddDomainEvent(new TaskUpdatedEvent(Id, Title, modifierUserId,
            new Dictionary<string, object?> { { nameof(IsArchived), true } },
            new Dictionary<string, object?> { { nameof(IsArchived), false } }
        ));
    }

    public void AddActivity(Activity activity)
    {
        if (activity == null)
            throw new ArgumentNullException(nameof(activity));

        _activities.Add(activity);
    }
}