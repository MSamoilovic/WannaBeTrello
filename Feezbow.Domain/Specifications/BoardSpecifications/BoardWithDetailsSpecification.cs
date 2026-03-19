using Feezbow.Domain.Entities;
using Feezbow.Domain.Specifications;

namespace Feezbow.Domain.Specifications.BoardSpecifications;


public class BoardWithDetailsSpecification : BaseSpecification<Board>
{
    public BoardWithDetailsSpecification(long boardId) 
        : base(b => b.Id == boardId)
    {
        
        AddInclude("Columns.Tasks.Assignee");
        AddInclude("Columns.Tasks.Comments");
        AddInclude("BoardMembers.User");
        
    }
}

