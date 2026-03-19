using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Features.Labels.RemoveLabelFromTask;

public class RemoveLabelFromTaskCommandHandler(
    IBoardTaskRepository taskRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    ICacheService cacheService) : IRequestHandler<RemoveLabelFromTaskCommand, RemoveLabelFromTaskCommandResponse>
{
    public async Task<RemoveLabelFromTaskCommandResponse> Handle(RemoveLabelFromTaskCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var task = await taskRepository.GetTaskWithLabelsAsync(request.TaskId, cancellationToken);
        if (task is null)
            throw new KeyNotFoundException($"Task {request.TaskId} not found.");

        task.RemoveLabel(request.LabelId, currentUserService.UserId.Value);
        await unitOfWork.CompleteAsync(cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.Task(request.TaskId), cancellationToken);

        return new RemoveLabelFromTaskCommandResponse(true, "Label removed from task.");
    }
}
