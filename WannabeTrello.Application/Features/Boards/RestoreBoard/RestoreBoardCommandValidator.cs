using System.Data;
using FluentValidation;

namespace WannabeTrello.Application.Features.Boards.RestoreBoard;

public class RestoreBoardCommandValidator: AbstractValidator<RestoreBoardCommand>
{
    public RestoreBoardCommandValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty()
            .WithMessage("The board id cannot be empty.")
            .GreaterThan(0)
            .WithMessage("The board id must be greater than 0.");;
    }
}