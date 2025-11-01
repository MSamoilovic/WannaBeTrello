using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Specification;

namespace WannabeTrello.Domain.Specifications.BoardSpecifications;

/// <summary>
/// Specification za dohvatanje svih Board-ova određenog projekta
/// </summary>
public class BoardsByProjectSpecification : BaseSpecification<Board>
{
    public BoardsByProjectSpecification(long projectId, bool includeArchived = false) 
        : base(b => b.ProjectId == projectId && (includeArchived || !b.IsArchived))
    {
        // Sortiranje po datumu kreiranja
        ApplyOrderBy(b => b.CreatedAt);
        
        // Opciono include members
        AddInclude(b => b.BoardMembers);
    }
}

