namespace WannabeTrello.Domain.Entities;

public class Project: AuditableEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public long OwnerId { get; set; } 
    public User Owner { get; set; } = null!; 
    public ICollection<Board?> Boards { get; set; } = [];
    public ICollection<ProjectMember> ProjectMembers { get; set; } = [];
}