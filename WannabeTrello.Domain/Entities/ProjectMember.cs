using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Domain.Entities;

public class ProjectMember
{
    public long ProjectId { get; init; }
    public Project Project { get; init; } = null!;

    public long? UserId { get; init; }
    public User User { get; private set; } = null!;

    public ProjectRole Role { get; private set; } 
    
    private ProjectMember() { }
    
    public static ProjectMember Create(long userId, long projectId, ProjectRole role)
    {
        if (userId <= 0)
            throw new ArgumentException("User ID must be greater than zero.", nameof(userId));
        
        var member = new ProjectMember
        {
            UserId = userId,
            ProjectId = projectId,
            Role = role
        };
        
        return member;
    }
    
    public void UpdateRole(ProjectRole newRole)
    {
        Role = newRole;
    }
}