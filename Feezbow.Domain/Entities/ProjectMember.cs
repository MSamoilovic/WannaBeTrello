using Feezbow.Domain.Enums;

namespace Feezbow.Domain.Entities;

public class ProjectMember
{
    public long ProjectId { get; init; }
    public Project Project { get; init; } = null!;

    public long? UserId { get; init; }
    public User User { get; private set; } = null!;

    public ProjectRole Role { get; private set; }

    /// <summary>
    /// Household-specific role. Null when the project is not a household.
    /// </summary>
    public HouseholdRole? HouseholdRole { get; private set; }

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

    public void SetHouseholdRole(HouseholdRole? householdRole)
    {
        HouseholdRole = householdRole;
    }
}