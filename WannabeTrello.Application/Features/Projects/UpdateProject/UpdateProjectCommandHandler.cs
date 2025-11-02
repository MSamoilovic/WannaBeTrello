using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Projects.UpdateProject;

public class UpdateProjectCommandHandler(IProjectService projectService, ICurrentUserService currentUserService)
    : IRequestHandler<UpdateProjectCommand, UpdateProjectCommandResponse>
{
    public async Task<UpdateProjectCommandResponse> Handle(UpdateProjectCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var project = await projectService.UpdateProjectAsync(
            request.ProjectId,
            request.Name,
            request.Description,
            request.Status,
            request.Visibility,
            request.Archived,
            currentUserService.UserId.Value, cancellationToken);

        return new UpdateProjectCommandResponse(
            project.Name,
            project.Description,
            project.Visibility,
            project.Status,
            project.IsArchived
        );
    }
}