using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Services;

namespace WannabeTrello.Application.Features.Projects.ArchiveProject;

internal class ArchiveProjectCommandHandler(
    ProjectService service,
    ICurrentUserService currentUserService
): IRequestHandler<ArchiveProjectCommand, ArchiveProjectCommandResponse>
{
    public async Task<ArchiveProjectCommandResponse> Handle(ArchiveProjectCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }
        
        var projectId = await service.ArchiveProjectAsync(request.ProjectId, currentUserService.UserId.Value);
        var result = Result<long>.Success(projectId, $"Project {request.ProjectId} is now archived.");
        
        return new ArchiveProjectCommandResponse(result);
    }
}