using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;
using WannabeTrello.Domain.Services;

namespace WannabeTrello.Application.Features.Projects.CreateProject;

public class CreateProjectCommandHandler(ICurrentUserService currentUserService, IProjectService projectService )
    : IRequestHandler<CreateProjectCommand, CreateProjectCommandResponse>
{
    public async Task<CreateProjectCommandResponse> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        
        var project = await projectService.CreateProjectAsync(request.Name, request.Description, currentUserService.UserId ?? 0
, cancellationToken);
        
        var result = Result<long>.Success(project.Id, "Project Created Successfully");
        
        return new CreateProjectCommandResponse(result);
    }
}