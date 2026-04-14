using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;

namespace Feezbow.Application.Features.Tasks.ClearTaskRecurrence;

public class ClearTaskRecurrenceCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<ClearTaskRecurrenceCommand, ClearTaskRecurrenceCommandResponse>
{
    public async Task<ClearTaskRecurrenceCommandResponse> Handle(
        ClearTaskRecurrenceCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var task = await unitOfWork.Tasks.GetTaskDetailsByIdAsync(request.TaskId, cancellationToken)
            ?? throw new NotFoundException("Task", request.TaskId);

        task.ClearRecurrence(currentUserService.UserId ?? 0);

        await unitOfWork.CompleteAsync(cancellationToken);

        return new ClearTaskRecurrenceCommandResponse(
            Result<bool>.Success(true, "Recurrence cleared."));
    }
}
