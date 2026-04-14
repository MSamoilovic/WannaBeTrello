using Feezbow.Domain.Enums;
using Feezbow.Domain.Events.TaskEvents;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.ValueObjects;

namespace Feezbow.Domain.Entities;

public class BoardTask : AuditableEntity
{
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int Position { get; private set; }
    public TaskPriority Priority { get; private set; } = TaskPriority.Medium;
    public TaskType TaskType { get; private set; } = TaskType.General;
    public DateTime? DueDate { get; private set; }

    public long ColumnId { get; private set; }
    public Column Column { get; private set; } = null!;

    public long? AssigneeId { get; private set; }
    public User? Assignee { get; private set; }

    public bool IsArchived { get; private set; }

    // --- Recurring task fields ---
    public RecurrenceRule? Recurrence { get; private set; }
    public DateTime? NextOccurrence { get; private set; }

    /// <summary>ID of the original task this was generated from. Null for root tasks.</summary>
    public long? ParentTaskId { get; private set; }
    public BoardTask? ParentTask { get; private set; }

    public bool IsRecurring => Recurrence is not null;

    public ICollection<Comment> Comments { get; private set; } = new List<Comment>();

    private readonly List<BoardTaskLabel> _taskLabels = [];
    public IReadOnlyCollection<BoardTaskLabel> TaskLabels => _taskLabels.AsReadOnly();

    private readonly List<Activity> _activities = [];
    public IReadOnlyCollection<Activity> Activities => _activities.AsReadOnly();

    private BoardTask()
    {
    }

    public static BoardTask Create(string title, string? description, TaskPriority priority, DateTime? dueDate,
        int position, long columnId, long? assigneeId, long creatorUserId, TaskType taskType = TaskType.General)
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
            TaskType = taskType,
            DueDate = dueDate,
            Position = position,
            ColumnId = columnId,
            AssigneeId = assigneeId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = creatorUserId
        };

        var activity = new Activity(
            ActivityType.TaskCreated,
            $"Task '{title}' was created",
            creatorUserId
        );

        task.AddActivity(activity);

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

        var activity = new Activity(
            ActivityType.TaskUpdated,
            $"Task '{newTitle}' was updated",
            modifierUserId,
            oldValues,
            newValues
        );

        AddActivity(activity);

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

        var activity = new Activity(
            ActivityType.TaskMoved,
            $"Task moved from column {originalColumnId} to column {newColumnId}",
            performingUserId,
            new Dictionary<string, object?> { ["OldColumnId"] = originalColumnId },
            new Dictionary<string, object?> { ["NewColumnId"] = newColumnId }
        );

        AddActivity(activity);

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

        var activity = new Activity(
            ActivityType.CommentAdded,
            $"Comment added to task",
            userId,
            newValue: new Dictionary<string, object?> { ["CommentId"] = comment.Id }
        );

        AddActivity(activity);

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

        var activity = new Activity(
            ActivityType.TaskAssigned,
            newAssigneeId.HasValue
                ? $"Task assigned to user {newAssigneeId}"
                : "Task unassigned",
            performingUserId,
            new Dictionary<string, object?> { ["OldAssigneeId"] = oldAssigneeId },
            new Dictionary<string, object?> { ["NewAssigneeId"] = newAssigneeId }
        );

        AddActivity(activity);

        AddDomainEvent(new TaskAssignedEvent(Id, oldAssigneeId, newAssigneeId, performingUserId));
    }

    public void SetPosition(int newPosition, long modifierUserId)
    {
        if (newPosition < 0)
            throw new BusinessRuleValidationException("Position cannot be negative.");
        if (newPosition == Position) return;
        
        var old = Position;
        Position = newPosition;

        var activity = new Activity(
            ActivityType.TaskUpdated,
            $"Task position changed from {old} to {newPosition}",
            modifierUserId,
            new Dictionary<string, object?> { [nameof(Position)] = old },
            new Dictionary<string, object?> { [nameof(Position)] = newPosition }
        );
        AddActivity(activity);

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

        var activity = new Activity(
            ActivityType.TaskUpdated,
            "Task archived",
            modifierUserId,
            new Dictionary<string, object?> { [nameof(IsArchived)] = false },
            new Dictionary<string, object?> { [nameof(IsArchived)] = true }
        );

        AddActivity(activity);
        
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

        var activity = new Activity(
            ActivityType.TaskUpdated,
            "Task restored",
            modifierUserId,
            new Dictionary<string, object?> { [nameof(IsArchived)] = true },
            new Dictionary<string, object?> { [nameof(IsArchived)] = false }
        );

        AddActivity(activity);

        AddDomainEvent(new TaskUpdatedEvent(Id, Title, modifierUserId,
            new Dictionary<string, object?> { { nameof(IsArchived), true } },
            new Dictionary<string, object?> { { nameof(IsArchived), false } }
        ));
    }

    public void AddLabel(Label label, long userId)
    {
        if (_taskLabels.Any(tl => tl.LabelId == label.Id))
            throw new BusinessRuleValidationException($"Label '{label.Name}' is already on this task.");

        _taskLabels.Add(new BoardTaskLabel { TaskId = Id, LabelId = label.Id });
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = userId;
    }

    public void RemoveLabel(long labelId, long userId)
    {
        var taskLabel = _taskLabels.FirstOrDefault(tl => tl.LabelId == labelId);
        if (taskLabel is null)
            throw new BusinessRuleValidationException("This label is not on the task.");

        _taskLabels.Remove(taskLabel);
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = userId;
    }

    public void SetRecurrence(RecurrenceRule rule, DateTime? firstOccurrence, long modifierUserId)
    {
        Recurrence = rule ?? throw new ArgumentNullException(nameof(rule));
        NextOccurrence = firstOccurrence;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifierUserId;

        var activity = new Activity(
            ActivityType.TaskUpdated,
            $"Recurrence set to {rule.Frequency} (every {rule.Interval})",
            modifierUserId,
            newValue: new Dictionary<string, object?>
            {
                ["Frequency"] = rule.Frequency.ToString(),
                ["Interval"] = rule.Interval,
                ["NextOccurrence"] = firstOccurrence
            });

        AddActivity(activity);
    }

    public void ClearRecurrence(long modifierUserId)
    {
        if (Recurrence is null) return;

        Recurrence = null;
        NextOccurrence = null;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifierUserId;

        AddActivity(new Activity(
            ActivityType.TaskUpdated,
            "Recurrence removed",
            modifierUserId,
            oldValue: new Dictionary<string, object?> { ["Recurrence"] = "cleared" }));
    }

    public void ScheduleNextOccurrence(DateTime nextDate, long systemUserId)
    {
        NextOccurrence = nextDate;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = systemUserId;
    }

    /// <summary>
    /// Creates a new one-time occurrence of this recurring task in the same column.
    /// The new task inherits all properties but has no recurrence of its own.
    /// </summary>
    public BoardTask SpawnOccurrence(long systemUserId)
    {
        if (Recurrence is null)
            throw new InvalidOperationDomainException("Cannot spawn occurrence from a non-recurring task.");

        var occurrence = new BoardTask
        {
            Title = Title,
            Description = Description,
            Priority = Priority,
            TaskType = TaskType,
            DueDate = NextOccurrence,
            Position = Position,
            ColumnId = ColumnId,
            AssigneeId = AssigneeId,
            ParentTaskId = Id,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = systemUserId
        };

        occurrence.AddActivity(new Activity(
            ActivityType.TaskCreated,
            $"Recurring occurrence generated from task '{Title}'",
            systemUserId));

        return occurrence;
    }

    public void AddActivity(Activity activity)
    {
        if (activity == null)
            throw new ArgumentNullException(nameof(activity));

        _activities.Add(activity);
    }
}