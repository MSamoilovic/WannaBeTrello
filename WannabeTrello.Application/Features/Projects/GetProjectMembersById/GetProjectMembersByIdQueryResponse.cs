using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Application.Features.Projects.GetProjectMembersById;

public record GetProjectMembersByIdQueryResponse(
    long UserId,
    string? FirstName,
    string? LastName,
    ProjectRole Role
);