using FluentValidation;

namespace WannabeTrello.Application.Features.Boards.ArchiveBoard;

public class ArchiveBoardCommandValidator : AbstractValidator<ArchiveBoardCommand>
{
    public ArchiveBoardCommandValidator()
    {
        RuleFor(x => x.BoardId).GreaterThan(0).WithMessage("BoardId must be greater than 0");
    }
}