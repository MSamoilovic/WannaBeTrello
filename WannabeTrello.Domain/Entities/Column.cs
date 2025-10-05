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


}