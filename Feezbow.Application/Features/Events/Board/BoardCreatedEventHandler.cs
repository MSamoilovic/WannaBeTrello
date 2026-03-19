using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Board_Events;

namespace Feezbow.Application.Features.Events.Board;

public class BoardCreatedEventHandler(
    IBoardNotificationService notificationService)
    : INotificationHandler<BoardCreatedEvent>
{
    public async Task Handle(BoardCreatedEvent notification, CancellationToken cancellationToken)
    {
        await notificationService.NotifyBoardCreated(notification.BoardId, notification.ProjectId,
            notification.BoardName, notification.CreatorUserId);
    }
}