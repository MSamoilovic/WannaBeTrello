using System.Data;
using FluentValidation;

namespace WannabeTrello.Application.Features.Comments.DeleteComment;

public class DeleteCommentCommandValidator : AbstractValidator<DeleteCommentCommand>
{
    public DeleteCommentCommandValidator()
    {
        RuleFor(x => x.CommentId).NotNull().NotEmpty().GreaterThan(0)
            .WithMessage("CommentId must be greater than zero");
    }
}