using FluentValidation;

namespace WannabeTrello.Application.Features.Boards.GetBoardById;

public class GetBoardByIdQueryValidator : AbstractValidator<GetBoardByIdQuery>
{
    public GetBoardByIdQueryValidator()
    {
        RuleFor(x => x.BoardId)
            .NotNull()
            .GreaterThan(0)
            .NotEmpty()
            .WithMessage("BoardId must be greater than zero");
    }
}