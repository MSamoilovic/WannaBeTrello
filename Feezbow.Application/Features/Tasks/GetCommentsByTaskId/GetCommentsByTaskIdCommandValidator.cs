using FluentValidation;

namespace Feezbow.Application.Features.Tasks.GetCommentsByTaskId;

public class GetCommentsByTaskIdCommandValidator : AbstractValidator<GetCommentsByTaskIdCommand>
{
    public GetCommentsByTaskIdCommandValidator()
    {
        RuleFor(x => x.TaskId).NotNull().GreaterThan(0).WithMessage("TaskId must be greater than zero");
    }
}