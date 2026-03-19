using Feezbow.Domain.Entities;
using Feezbow.Domain.Specifications;

namespace Feezbow.Domain.Specifications.BoardSpecifications;

/// <summary>
/// Specification za dohvatanje svih Board-ova određenog projekta
/// </summary>
public class BoardsByProjectSpecification : BaseSpecification<Board>
{
    public BoardsByProjectSpecification(long projectId, bool includeArchived = false) 
        : base(b => b.ProjectId == projectId && (includeArchived || !b.IsArchived))
    {
       
        ApplyOrderBy(b => b.CreatedAt);
        AddInclude(b => b.BoardMembers);
    }
}

