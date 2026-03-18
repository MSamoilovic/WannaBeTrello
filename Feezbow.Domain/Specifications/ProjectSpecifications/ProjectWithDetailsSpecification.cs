using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Specifications.ProjectSpecifications;

/// <summary>
/// Specification za dobijanje projekta sa svim detaljima (members, boards)
/// </summary>
public class ProjectWithDetailsSpecification : BaseSpecification<Project>
{
    public ProjectWithDetailsSpecification(long projectId) 
        : base(p => p.Id == projectId)
    {
        AddInclude("ProjectMembers.User");
        AddInclude("Boards");
    }
}

