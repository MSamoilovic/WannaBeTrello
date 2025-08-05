namespace WannabeTrello.Domain.Entities;

public class Column: AuditableEntity
{
    public string Name { get; set; } = null!;
    public int Order { get; set; } 
    public long BoardId { get; set; }
    public Board Board { get; set; } = null!;
    public ICollection<BoardTask> Tasks { get; set; } = new List<BoardTask>();
}