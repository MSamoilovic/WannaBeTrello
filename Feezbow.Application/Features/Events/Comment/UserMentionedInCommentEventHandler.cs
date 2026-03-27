using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Comment_Events;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Features.Events.Comment;

internal class UserMentionedInCommentEventHandler(
    IUserRepository userRepository,
    IUserNotificationService userNotificationService,
    ILogger<UserMentionedInCommentEventHandler> logger)
    : INotificationHandler<UserMentionedInCommentEvent>
{
    public async Task Handle(UserMentionedInCommentEvent notification, CancellationToken cancellationToken)
    {
        var usernames = notification.MentionedUsernames.ToList();

        var mentionedUserIds = await userRepository.SearchUsers()
            .Where(u => u.IsActive && usernames.Contains(u.UserName!))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        foreach (var userId in mentionedUserIds)
        {
            if (userId == notification.MentionedByUserId)
                continue;

            logger.LogInformation(
                "Notifying user {MentionedUserId} of mention in task {TaskId} by user {MentionedByUserId}",
                userId, notification.TaskId, notification.MentionedByUserId);

            await userNotificationService.NotifyUserMentioned(
                userId,
                notification.TaskId,
                notification.MentionedByUserId,
                cancellationToken);
        }
    }
}
