using FluentValidation;

namespace WannabeTrello.Application.Features.Projects.AddProjectMember;

public class AddProjectMemberCommandValidator: AbstractValidator<AddProjectMemberCommand>
{
    public AddProjectMemberCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotNull()
            .WithMessage("Project ID cannot be empty.");
        
        RuleFor(x => x.NewMemberId)
            .NotNull()
            .WithMessage("New Member ID cannot be empty.");
    }
}