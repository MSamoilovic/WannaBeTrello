using WannabeTrello.Domain.Events.Column_Events;
using WannabeTrello.Domain.Exceptions;

namespace WannabeTrello.Domain.Entities;

public class Column: AuditableEntity
{
    public string? Name { get; private set; }
    public int Order { get; private set; } 
    public long BoardId { get; private set; }
    public Board Board { get; private set; } = null!;
    
    public int? WipLimit { get; private set; }
    
    private readonly List<BoardTask> _tasks = [];
    public IReadOnlyCollection<BoardTask> Tasks => _tasks.AsReadOnly();
    
    public bool IsDeleted { get; private set; }
    
    private Column () {}

    internal Column(string? name, long boardId, int order, long userId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleValidationException("Column name cannot be empty.");
        if (order <= 0)
            throw new BusinessRuleValidationException("Column order must be a positive number.");

        Name = name;
        BoardId = boardId;
        Order = order;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = userId;
    }

    public void ChangeName(string newName, long modifierUserId)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new BusinessRuleValidationException("New column name cannot be empty.");

        var oldName = Name;
        Name = newName;

        AddDomainEvent(new ColumnUpdatedEvent(Id, oldName!, newName, BoardId, modifierUserId));
    }

    public void ChangeOrder(int newOrder, long modifierUserId)
    {
        if (newOrder <= 0)
            throw new BusinessRuleValidationException("New column order must be a positive number.");

        var oldOrder = Order;
        Order = newOrder;

        AddDomainEvent(new ColumnOrderChangedEvent(Id, BoardId, oldOrder, newOrder, modifierUserId));
    }

    public void SetWipLimit(int? limit, long modifierUserId)
    {
        if (limit is <= 0)
            throw new BusinessRuleValidationException("WIP limit must be a positive number.");

        var oldWipLimit = WipLimit;
        WipLimit = limit;

        AddDomainEvent(new ColumnWipLimitChangedEvent(Id, BoardId, oldWipLimit, limit, modifierUserId));
    }
    
    internal void AddTask(BoardTask task)
    {
        if (IsWipLimitReached())
            throw new BusinessRuleValidationException($"WIP limit for column '{Name}' has been reached.");
        _tasks.Add(task);
    }

    internal BoardTask RemoveTask(long taskId)
    {
        var taskToRemove = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (taskToRemove == null)
            throw new NotFoundException(nameof(BoardTask), taskId);
        _tasks.Remove(taskToRemove);
        return taskToRemove;
    }

    internal bool HasTask(long taskId)
    {
        return _tasks.Any(t => t.Id == taskId);
    }

    private bool IsWipLimitReached()
    {
        if (!WipLimit.HasValue) return false;
        return _tasks.Count >= WipLimit.Value;
    }

    public void DeleteColumn(long modifierUserId)
    {
        if (IsDeleted)
            return;
            
        IsDeleted = true;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifierUserId;
        
        AddDomainEvent(new ColumnDeletedEvent(Id, BoardId, modifierUserId));
        
        
    }
}