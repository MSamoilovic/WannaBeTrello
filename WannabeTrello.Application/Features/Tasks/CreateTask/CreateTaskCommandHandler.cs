using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Services;

namespace WannabeTrello.Application.Features.Tasks.CreateTask;

public class CreateTaskCommandHandler(BoardTaskService taskService, ICurrentUserService currentUserService)
    : IRequestHandler<CreateTaskCommand, long>
{
    public async Task<long> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("Korisnik nije autentifikovan.");
        }

        var task = await taskService.CreateTaskAsync(
            request.ColumnId,
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate,
            request.Position,
            request.AssigneeId,
            currentUserService.UserId.Value
        );

        return task.Id;
    }
}
