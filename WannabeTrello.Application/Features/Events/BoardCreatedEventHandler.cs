using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events.Board_Events;


namespace WannabeTrello.Application.Features.Events;

public class BoardCreatedEventHandler(IBoardNotificationService notificationService)
    : INotificationHandler<BoardCreatedEvent>
{
    public async Task Handle(BoardCreatedEvent notification, CancellationToken cancellationToken)
    {
        await notificationService.NotifyBoardCreated(notification.BoardId, notification.BoardName);
    }
}