using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events.Board_Events;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Events.Board;

public class BoardRestoredEventHandler(
    IBoardNotificationService boardNotificationService,
    IActivityTrackerService activityTrackerService) : INotificationHandler<BoardRestoredEvent>
{
    public async Task Handle(BoardRestoredEvent notification, CancellationToken cancellationToken)
    {
        await boardNotificationService.NotifyBoardRestored(notification.BoardId, notification.ModifierUserId);
    }
}