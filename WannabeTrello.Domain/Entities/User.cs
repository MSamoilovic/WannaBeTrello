using Microsoft.AspNetCore.Identity;

namespace WannabeTrello.Domain.Entities;
public class User : IdentityUser<long>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public ICollection<Project> OwnedProjects { get; set; } = new List<Project>();
    public ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();
    public ICollection<BoardMember> BoardMemberships { get; set; } = new List<BoardMember>();
    public ICollection<BoardTask> AssignedTasks { get; set; } = new List<BoardTask>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}