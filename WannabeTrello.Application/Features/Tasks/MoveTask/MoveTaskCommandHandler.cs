using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Services;

namespace WannabeTrello.Application.Features.Tasks.MoveTask;

public class MoveTaskCommandHandler(BoardTaskService taskService, ICurrentUserService currentUserService)
    : IRequestHandler<MoveTaskCommand, Unit>
{
    public async Task<Unit> Handle(MoveTaskCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("Korisnik nije autentifikovan.");
        }

        await taskService.MoveTaskAsync(
            request.TaskId,
            request.NewColumnId,
            currentUserService.UserId.Value
        );

        return Unit.Value; // Označava uspešno izvršenje
    }
}