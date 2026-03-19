using MediatR;
using Feezbow.Domain.Enums;

namespace Feezbow.Application.Features.Projects.UpdateProjectMemberRole;

public record UpdateProjectMemberRoleCommand(long ProjectId, long MemberId, ProjectRole Role)
    : IRequest<UpdateProjectMemberRoleCommandResponse>;