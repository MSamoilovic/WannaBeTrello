using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Specifications.TaskSpecifications;

public class TasksByBoardIdSpecification: BaseSpecification<BoardTask>
{
    public TasksByBoardIdSpecification(long boardId) : base(t => t.Column.BoardId == boardId)
    {
        AddInclude(t => t.Column);
        ApplyOrderBy(t => t.CreatedAt);
        ApplyTracking();
    }
}