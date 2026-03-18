using FluentValidation;

namespace WannabeTrello.Application.Features.Comments.RestoreComment;

public class RestoreCommentCommandValidator: AbstractValidator<RestoreCommentCommand>
{
    public RestoreCommentCommandValidator()
    {
        RuleFor(x => x.CommentId).NotNull().NotEmpty().GreaterThan(0)
            .WithMessage("CommentId must be greater than zero");
    }
}