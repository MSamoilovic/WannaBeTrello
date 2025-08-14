using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Domain.Entities;

public class ProjectMember
{
    public long ProjectId { get; init; }
    public Project Project { get; init; } = null!;

    public long? UserId { get; init; }
    public User User { get; private set; } = null!;

    public ProjectRole Role { get; init; } 
}