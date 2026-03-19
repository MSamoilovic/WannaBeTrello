using Feezbow.Domain.Enums;

namespace Feezbow.Application.Features.Projects.GetProjectMembersById;

public record GetProjectMembersByIdQueryResponse(
    long UserId,
    string? FirstName,
    string? LastName,
    ProjectRole Role
);