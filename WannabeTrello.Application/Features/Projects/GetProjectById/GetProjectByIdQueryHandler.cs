using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Services;
using WannabeTrello.Domain.Services;

namespace WannabeTrello.Application.Features.Projects.GetProjectById;

public class GetProjectByIdQueryHandler(IProjectService projectService, ICurrentUserService currentUserService)
    : IRequestHandler<GetProjectByIdQuery, GetProjectByIdQueryResponse>
{
    public async Task<GetProjectByIdQueryResponse> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new AccessDeniedException("Nemate pristup za pregled ovog boarda.");
        }
        
        var project = await projectService.GetProjectByIdAsync(request.ProjectId, currentUserService.UserId.Value, cancellationToken);
        
        return GetProjectByIdQueryResponse.FromEntity(project);
    }
}