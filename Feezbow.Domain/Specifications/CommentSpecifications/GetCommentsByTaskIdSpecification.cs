using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Specifications.CommentSpecifications;

public class GetCommentsByTaskIdSpecification: BaseSpecification<Comment>
{
    public GetCommentsByTaskIdSpecification(long taskId, bool includeDeleted = false) 
        : base(x => x.TaskId == taskId && (includeDeleted || !x.IsDeleted))
    {
        AddInclude(comment => comment.Task);
        AddInclude(comment => comment.User);
        ApplyOrderBy(t => t.CreatedAt);
    }
}