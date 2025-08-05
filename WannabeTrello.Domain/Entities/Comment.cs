namespace WannabeTrello.Domain.Entities;

public class Comment: AuditableEntity
{
    public string Content { get; set; } = null!;
    public long TaskId { get; set; }
    public BoardTask Task { get; set; } = null!; 
    public long UserId { get; set; } 
    public User User { get; set; } = null!; 
    
    //Izmeniti domain model da ima svoje aktivnosti
}