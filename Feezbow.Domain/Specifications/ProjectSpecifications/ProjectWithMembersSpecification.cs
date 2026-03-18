using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Specifications.ProjectSpecifications;

/// <summary>
/// Specification za dobijanje projekta samo sa članovima (bez board-ova)
/// </summary>
public class ProjectWithMembersSpecification : BaseSpecification<Project>
{
    public ProjectWithMembersSpecification(long projectId) 
        : base(p => p.Id == projectId)
    {
        AddInclude("ProjectMembers.User");
        ApplyTracking();
    }
}

