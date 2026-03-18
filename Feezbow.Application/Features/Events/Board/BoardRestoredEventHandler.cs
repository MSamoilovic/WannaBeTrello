using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events.Board_Events;

namespace WannabeTrello.Application.Features.Events.Board;

public class BoardRestoredEventHandler(
    IBoardNotificationService boardNotificationService) : INotificationHandler<BoardRestoredEvent>
{
    public async Task Handle(BoardRestoredEvent notification, CancellationToken cancellationToken)
    {
        await boardNotificationService.NotifyBoardRestored(notification.BoardId, notification.ModifierUserId);
    }
}