﻿namespace WannabeTrello.Application.Common.Interfaces;

public interface IBoardNotificationService
{
    Task NotifyBoardCreated(long createdBoardId, string boardName);
    Task NotifyBoardUpdated(long createdBoardId, long ModifierUserId);
}