using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events;

namespace WannabeTrello.Application.Features.Events.Board;

public class BoardUpdatedEventHandler(IBoardNotificationService notificationService)
    : INotificationHandler<BoardUpdatedEvent>
{
    public async Task Handle(BoardUpdatedEvent notification, CancellationToken cancellationToken)
    {
        await notificationService.NotifyBoardUpdated(notification.BoardId, notification.ModifierUserId);
    }
}