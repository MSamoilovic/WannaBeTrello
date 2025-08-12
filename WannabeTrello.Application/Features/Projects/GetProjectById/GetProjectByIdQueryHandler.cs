using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Services;

namespace WannabeTrello.Application.Features.Projects.GetProjectById;

internal class GetProjectByIdQueryHandler(ProjectService projectService, ICurrentUserService currentUserService)
    : IRequestHandler<GetProjectByIdQuery, GetProjectByIdQueryResponse>
{
    public async Task<GetProjectByIdQueryResponse> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new AccessDeniedException("Nemate pristup za pregled ovog boarda.");
        }
        
        var project = await projectService.GetProjectByIdAsync(request.ProjectId, currentUserService.UserId.Value);
        
        return GetProjectByIdQueryResponse.FromEntity(project);
    }
}