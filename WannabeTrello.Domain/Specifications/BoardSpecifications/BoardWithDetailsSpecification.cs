using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Specification;

namespace WannabeTrello.Domain.Specifications.BoardSpecifications;

/// <summary>
/// Specification za dohvatanje Board-a sa svim detaljima (Columns, Tasks, Members, Comments)
/// </summary>
public class BoardWithDetailsSpecification : BaseSpecification<Board>
{
    public BoardWithDetailsSpecification(long boardId) 
        : base(b => b.Id == boardId)
    {
        // Eager loading svih related entiteta
        AddInclude("Columns.Tasks.Assignee");
        AddInclude("Columns.Tasks.Comments");
        AddInclude("BoardMembers.User");
        
        // AsNoTracking je default (true) jer je ovo read-only query
    }
}

