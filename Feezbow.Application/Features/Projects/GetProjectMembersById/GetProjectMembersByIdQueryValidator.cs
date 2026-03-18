using FluentValidation;

namespace WannabeTrello.Application.Features.Projects.GetProjectMembersById;

public class GetProjectMembersByIdQueryValidator: AbstractValidator<GetProjectMembersByIdQuery>
{
    public GetProjectMembersByIdQueryValidator()
    {
        RuleFor(x => x.ProjectId).NotNull().GreaterThan(0);
    }
}