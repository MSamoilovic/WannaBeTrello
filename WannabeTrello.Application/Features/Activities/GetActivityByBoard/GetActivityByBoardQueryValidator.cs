using FluentValidation;

namespace WannabeTrello.Application.Features.Activities.GetActivityByBoard;

public class GetActivityByBoardQueryValidator: AbstractValidator<GetActivityByBoardQuery>
{
    public GetActivityByBoardQueryValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty()
            .WithMessage("BoardId cannot be empty")
            .GreaterThan(0)
            .WithMessage("BoardId must be greather than 0");
    }
}
