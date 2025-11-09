using FluentValidation;

namespace WannabeTrello.Application.Features.Comments.UpdateCommentContent;

public class UpdateCommentContentCommandValidator : AbstractValidator<UpdateCommentContentCommand>
{
    public UpdateCommentContentCommandValidator()
    {
        RuleFor(x => x.CommentId).NotNull().NotEmpty().GreaterThan(0)
            .WithMessage("CommentId must be greater than zero.");

        RuleFor(x => x.NewContent).NotNull().NotEmpty().MaximumLength(300)
            .WithMessage("NewContent must be less than 300 characters.");
    }
}