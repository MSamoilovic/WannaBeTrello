using FluentValidation;

namespace WannabeTrello.Application.Features.Projects.UpdateProjectMemberRole;

public class UpdateProjectMemberRoleCommandValidator: AbstractValidator<UpdateProjectMemberRoleCommand>
{
    public UpdateProjectMemberRoleCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("ProjectId is required")
            .GreaterThan(0)
            .WithMessage("ProjectId must be greater than 0");
        
        RuleFor(x => x.MemberId)
            .NotEmpty()
            .WithMessage("MemberId is required")
            .GreaterThan(0)
            .WithMessage("MemberId must be greater than 0");

        RuleFor(x => x.Role)
            .NotEmpty()
            .WithMessage("Role is required");
    }
}