using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Specification;

namespace WannabeTrello.Domain.Specifications.BoardSpecifications;

/// <summary>
/// Specification za dohvatanje svih Board-ova gde korisnik ima Admin rolu
/// </summary>
public class BoardsWithAdminRoleSpecification : BaseSpecification<Board>
{
    public BoardsWithAdminRoleSpecification(long userId) 
        : base(b => b.BoardMembers.Any(bm => bm.UserId == userId && bm.Role == BoardRole.Admin))
    {
        AddInclude(b => b.Project);
        AddInclude(b => b.BoardMembers);
        
        ApplyOrderByDescending(b => b.LastModifiedAt ?? b.CreatedAt);
    }
}

