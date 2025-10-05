using FluentValidation;

namespace WannabeTrello.Application.Features.Boards.GetColumnsByBoardIId;

public class GetColumnsByBoardIdQueryValidator : AbstractValidator<GetColumnsByBoardIdQuery>
{
    public GetColumnsByBoardIdQueryValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty()
            .NotNull()
            .GreaterThan(0)
            .WithMessage("The board id must be greater than zero.");
    }
}