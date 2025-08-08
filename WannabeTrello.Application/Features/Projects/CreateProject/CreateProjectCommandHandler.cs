using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Services;

namespace WannabeTrello.Application.Features.Projects.CreateProject;

public class CreateProjectCommandHandler(ICurrentUserService currentUserService, ProjectService projectService )
    : IRequestHandler<CreateProjectCommand, long>
{
    public async Task<long> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        
        var project = await projectService.CreateProjectAsync(
            request.Name,
            request.Description,
            currentUserService.UserId ?? 0
        );

        return project.Id;
    }
}