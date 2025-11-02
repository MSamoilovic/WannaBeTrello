using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Specifications.ProjectSpecifications;

/// <summary>
/// Specification za dobijanje svih projekata za određenog korisnika (bez obzira na status)
/// </summary>
public class ProjectsByUserSpecification : BaseSpecification<Project>
{
    public ProjectsByUserSpecification(long userId) 
        : base(p => p.ProjectMembers.Any(pm => pm.UserId == userId))
    {
        AddInclude(p => p.ProjectMembers);
        ApplyOrderByDescending(p => p.CreatedAt);
    }
}

