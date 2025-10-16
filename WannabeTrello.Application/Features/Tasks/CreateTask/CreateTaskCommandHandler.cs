using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;
using WannabeTrello.Domain.Services;

namespace WannabeTrello.Application.Features.Tasks.CreateTask;

public class CreateTaskCommandHandler(IBoardTaskService taskService, ICurrentUserService currentUserService)
    : IRequestHandler<CreateTaskCommand, CreateTaskCommandResponse>
{
    public async Task<CreateTaskCommandResponse> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
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
            currentUserService.UserId.Value,
            cancellationToken
        );

        return new CreateTaskCommandResponse(Result<long>.Success(task.Id, "Task created successfully"));
    }
}
