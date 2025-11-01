using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Specification;

namespace WannabeTrello.Domain.Specifications.BoardSpecifications;


public class BoardWithDetailsSpecification : BaseSpecification<Board>
{
    public BoardWithDetailsSpecification(long boardId) 
        : base(b => b.Id == boardId)
    {
        // Eager loading svih related entiteta
        AddInclude("Columns.Tasks.Assignee");
        AddInclude("Columns.Tasks.Comments");
        AddInclude("BoardMembers.User");
        
    }
}

