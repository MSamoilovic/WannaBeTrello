using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Specifications.ProjectSpecifications;

/// <summary>
/// Specification za dobijanje članova projekta po projectId-u
/// </summary>
public class ProjectMembersByProjectIdSpecification : BaseSpecification<ProjectMember>
{
    public ProjectMembersByProjectIdSpecification(long projectId) 
        : base(pm => pm.ProjectId == projectId)
    {
        AddInclude(pm => pm.User);
        ApplyOrderBy(pm => pm.User.FirstName);
    }
}