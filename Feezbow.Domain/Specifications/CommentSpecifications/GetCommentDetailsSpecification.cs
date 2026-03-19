using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Specifications.CommentSpecifications;

public class GetCommentDetailsSpecification: BaseSpecification<Comment>
{
    public GetCommentDetailsSpecification(long id): base(comment => comment.Id == id)
    {
        AddInclude(comment => comment.Task);
        AddInclude(comment => comment.User);
    }
}