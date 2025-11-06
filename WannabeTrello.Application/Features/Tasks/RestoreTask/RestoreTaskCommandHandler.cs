using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Tasks.RestoreTask;

public class RestoreTaskCommandHandler(IBoardTaskService boardTaskService, ICurrentUserService currentUserService): IRequestHandler<RestoreTaskCommand, RestoreTaskCommandResponse>
{
    public async Task<RestoreTaskCommandResponse> Handle(RestoreTaskCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }
        
        var userId = currentUserService.UserId.Value;
        
        await boardTaskService.RestoreTaskAsync(request.TaskId, userId, cancellationToken);
        
        return new RestoreTaskCommandResponse(Result<long>.Success(request.TaskId, "Restored task"));
    }
}