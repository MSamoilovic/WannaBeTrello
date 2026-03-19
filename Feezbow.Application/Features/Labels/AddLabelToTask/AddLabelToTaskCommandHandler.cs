using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Features.Labels.AddLabelToTask;

public class AddLabelToTaskCommandHandler(
    IBoardTaskRepository taskRepository,
    ILabelRepository labelRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    ICacheService cacheService) : IRequestHandler<AddLabelToTaskCommand, AddLabelToTaskCommandResponse>
{
    public async Task<AddLabelToTaskCommandResponse> Handle(AddLabelToTaskCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var task = await taskRepository.GetTaskWithLabelsAsync(request.TaskId, cancellationToken);
        if (task is null)
            throw new KeyNotFoundException($"Task {request.TaskId} not found.");

        var label = await labelRepository.GetByIdAsync(request.LabelId, cancellationToken);
        if (label is null)
            throw new KeyNotFoundException($"Label {request.LabelId} not found.");

        var boardId = await taskRepository.GetBoardIdByTaskIdAsync(request.TaskId, cancellationToken);
        if (label.BoardId != boardId)
            throw new BusinessRuleValidationException("Label does not belong to the same board as the task.");

        task.AddLabel(label, currentUserService.UserId.Value);
        await unitOfWork.CompleteAsync(cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.Task(request.TaskId), cancellationToken);

        return new AddLabelToTaskCommandResponse(true, $"Label '{label.Name}' added to task.");
    }
}
