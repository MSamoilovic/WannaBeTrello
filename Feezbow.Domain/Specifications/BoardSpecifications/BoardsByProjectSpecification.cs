using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Specifications;

namespace WannabeTrello.Domain.Specifications.BoardSpecifications;

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

