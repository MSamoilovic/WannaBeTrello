﻿using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events.Board_Events;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Application.Features.Events.Board;

public class BoardCreatedEventHandler(
    IBoardNotificationService notificationService,
    IActivityTrackerRepository activityTrackerRepository)
    : INotificationHandler<BoardCreatedEvent>
{
    public async Task Handle(BoardCreatedEvent notification, CancellationToken cancellationToken)
    {
        await notificationService.NotifyBoardCreated(notification.BoardId, notification.BoardName,
            notification.CreatorUserId);
    }
}