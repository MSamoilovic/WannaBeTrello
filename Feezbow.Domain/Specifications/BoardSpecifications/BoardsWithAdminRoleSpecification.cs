using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;
using Feezbow.Domain.Specifications;

namespace Feezbow.Domain.Specifications.BoardSpecifications;

/// <summary>
/// Specification za dohvatanje svih Board-ova gde korisnik ima Admin rolu
/// </summary>
public class BoardsWithAdminRoleSpecification : BaseSpecification<Board>
{
    public BoardsWithAdminRoleSpecification(long userId) 
        : base(b => b.BoardMembers.Any(bm => bm.UserId == userId && bm.Role == BoardRole.Admin))
    {
        AddInclude(b => b.Project!);
        AddInclude(b => b.BoardMembers);
        
        ApplyOrderByDescending(b => (object)(b.LastModifiedAt ?? b.CreatedAt));
    }
}

