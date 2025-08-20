using System.Data;
using FluentValidation;

namespace WannabeTrello.Application.Features.Projects.RemoveProjectMember;

public class RemoveProjectMemberCommandValidator: AbstractValidator<RemoveProjectMemberCommand>
{
    public RemoveProjectMemberCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty().WithMessage("Project ID is required.");
        RuleFor(x => x.UserToRemoveId).NotEmpty().WithMessage("User ID is required.");
    }
}