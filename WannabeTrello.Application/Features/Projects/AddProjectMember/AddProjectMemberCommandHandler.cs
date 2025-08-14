using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Services;

namespace WannabeTrello.Application.Features.Projects.AddProjectMember;

internal class AddProjectMemberCommandHandler(
    ProjectService projectService,
    ICurrentUserService currentUserService
) : IRequestHandler<AddProjectMemberCommand, AddProjectMemberCommandResponse>
{
    public async Task<AddProjectMemberCommandResponse> Handle(AddProjectMemberCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var projectId = await projectService.AddProjectMember(
            request.ProjectId, 
            request.NewMemberId, 
            request.Role,
            currentUserService.UserId.Value);
        
        var result = Result<long>.Success(projectId, $"{request.NewMemberId} is now added to the project.");
        
        return new AddProjectMemberCommandResponse(result);
    }
}