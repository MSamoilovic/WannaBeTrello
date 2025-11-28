using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events.Board_Events;

namespace WannabeTrello.Application.Features.Events.Board;

public class BoardArchivedEventHandler(IBoardNotificationService boardNotificationService): INotificationHandler<BoardArchivedEvent>
{
    public async  Task Handle(BoardArchivedEvent notification, CancellationToken cancellationToken)
    {   
        await boardNotificationService.NotifyBoardArchived(notification.BoardId, notification.ModifierUserId);
    }
}