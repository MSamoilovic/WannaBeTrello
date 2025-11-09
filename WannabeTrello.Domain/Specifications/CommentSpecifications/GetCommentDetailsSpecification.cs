using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Specifications.CommentSpecifications;

public class GetCommentDetailsSpecification: BaseSpecification<Comment>
{
    public GetCommentDetailsSpecification(long id): base(comment => comment.Id == id)
    {
        AddInclude(comment => comment.Task);
        AddInclude(comment => comment.User);
    }
}