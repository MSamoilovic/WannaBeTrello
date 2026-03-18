using FluentValidation;

namespace WannabeTrello.Application.Features.Users.GetUserBoards;

public class GetUserBoardsQueryValidator : AbstractValidator<GetUserBoardsQuery>
{
    public GetUserBoardsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required")
            .GreaterThan(0)
            .WithMessage("UserId must be greater than 0");
    }
}

