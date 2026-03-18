using MediatR;
using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Application.Features.Projects.UpdateProjectMemberRole;

public record UpdateProjectMemberRoleCommand(long ProjectId, long MemberId, ProjectRole Role)
    : IRequest<UpdateProjectMemberRoleCommandResponse>;