using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Specifications.UserSpecifications;

public class GetUserProjectsSpecification : BaseSpecification<Project>
{
    public GetUserProjectsSpecification(long userId) 
        : base(p => p.ProjectMembers.Any(pm => pm.UserId == userId))
    {
        // Include related entities
        AddInclude(p => p.Owner);
        AddInclude(p => p.ProjectMembers);
        AddInclude(p => p.Boards);
        
        // Order by created date descending
        ApplyOrderByDescending(p => p.CreatedAt);
    }
}

