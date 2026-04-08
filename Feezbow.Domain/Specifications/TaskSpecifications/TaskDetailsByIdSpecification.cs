using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Specifications.TaskSpecifications;

public class TaskDetailsByIdSpecification: BaseSpecification<BoardTask>
{
    public TaskDetailsByIdSpecification(long id) : base(t => t.Id == id)
    {
        AddInclude(t => t.Column);
        AddInclude("Column.Board.BoardMembers");
        AddInclude(t => t.Assignee!);
        AddInclude(t => t.Comments);
    }
}