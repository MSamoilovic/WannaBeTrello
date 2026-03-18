using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Specifications.TaskSpecifications;

public class TaskDetailsByIdSpecification: BaseSpecification<BoardTask>
{
    public TaskDetailsByIdSpecification(long id) : base(t => t.Id == id)
    {
        AddInclude(t => t.Column);
        AddInclude(t => t.Assignee!);
        AddInclude(t => t.Comments);
        
    }
}