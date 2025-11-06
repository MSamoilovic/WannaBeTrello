using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Tasks.ArchiveTask;

public class ArchiveTaskCommandHandler(IBoardTaskService boardTaskService, ICurrentUserService currentUserService)
    : IRequestHandler<ArchiveTaskCommand, ArchiveTaskCommandResponse>
{
    public async Task<ArchiveTaskCommandResponse> Handle(ArchiveTaskCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }
        
        var userId = currentUserService.UserId.Value;
        
        await boardTaskService.ArchiveTaskAsync(request.TaskId, userId, cancellationToken);
        
        return new ArchiveTaskCommandResponse(Result<long>.Success(request.TaskId, "Archived task"));
    }
}