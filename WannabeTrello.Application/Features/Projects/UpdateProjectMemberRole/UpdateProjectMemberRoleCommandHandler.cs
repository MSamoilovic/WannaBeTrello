using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Projects.UpdateProjectMemberRole;

public class UpdateProjectMemberRoleCommandHandler(
    IProjectService projectService,
    ICurrentUserService currentUserService)
    : IRequestHandler<UpdateProjectMemberRoleCommand, UpdateProjectMemberRoleCommandResponse>
{
    public async Task<UpdateProjectMemberRoleCommandResponse> Handle(UpdateProjectMemberRoleCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        await projectService.UpdateProjectMember(
            request.ProjectId,
            request.MemberId,
            request.Role,
            currentUserService.UserId.Value
        );

        var result = Result<long>.Success(request.ProjectId, $"Project member {request.MemberId} role updated");
        return new UpdateProjectMemberRoleCommandResponse(result);
    }
}