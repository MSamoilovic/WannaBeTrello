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

    public void ChangeName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new BusinessRuleValidationException("New column name cannot be empty.");
        Name = newName;
    }

    public void ChangeOrder(int newOrder)
    {
        if (newOrder <= 0)
            throw new BusinessRuleValidationException("New column order must be a positive number.");
        Order = newOrder;
    }

    public void SetWipLimit(int? limit)
    {
        if (limit.HasValue && limit.Value <= 0)
            throw new BusinessRuleValidationException("WIP limit must be a positive number.");
        WipLimit = limit;
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
}