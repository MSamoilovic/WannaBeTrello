using FluentValidation;

namespace WannabeTrello.Application.Features.Activities.GetActivityByTask;

public class GetActivityByTaskQueryValidator : AbstractValidator<GetActivityByTaskQuery>
{
    public GetActivityByTaskQueryValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty()
            .WithMessage("TaskId cannot be empty")
            .GreaterThan(0)
            .WithMessage("TaskId must be greater than 0");
    }
}

