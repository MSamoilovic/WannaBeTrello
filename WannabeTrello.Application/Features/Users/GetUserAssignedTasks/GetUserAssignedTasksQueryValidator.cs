using FluentValidation;

namespace WannabeTrello.Application.Features.Users.GetUserAssignedTasks;

public class GetUserAssignedTasksQueryValidator : AbstractValidator<GetUserAssignedTasksQuery>
{
    public GetUserAssignedTasksQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required")
            .GreaterThan(0)
            .WithMessage("UserId must be greater than 0");
    }
}

