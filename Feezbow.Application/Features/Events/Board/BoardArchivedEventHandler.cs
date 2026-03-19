using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Board_Events;

namespace Feezbow.Application.Features.Events.Board;

public class BoardArchivedEventHandler(IBoardNotificationService boardNotificationService): INotificationHandler<BoardArchivedEvent>
{
    public async  Task Handle(BoardArchivedEvent notification, CancellationToken cancellationToken)
    {   
        await boardNotificationService.NotifyBoardArchived(notification.BoardId, notification.ModifierUserId);
    }
}