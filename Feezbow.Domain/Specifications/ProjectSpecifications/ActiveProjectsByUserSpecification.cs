using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Domain.Specifications.ProjectSpecifications;

/// <summary>
/// Specification za dobijanje aktivnih projekata za određenog korisnika
/// </summary>
public class ActiveProjectsByUserSpecification : BaseSpecification<Project>
{
    public ActiveProjectsByUserSpecification(long userId) 
        : base(p => p.ProjectMembers.Any(pm => pm.UserId == userId) && 
                    p.Status == ProjectStatus.Active)
    {
        AddInclude(p => p.ProjectMembers);
        ApplyOrderByDescending(p => p.CreatedAt);
    }
}

