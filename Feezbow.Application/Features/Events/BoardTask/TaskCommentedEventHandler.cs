using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.TaskEvents;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Features.Events.BoardTask;

internal class TaskCommentedEventHandler(
    ITaskNotificationService taskNotificationService,
    ICommentRepository commentRepository)
    : INotificationHandler<TaskCommentedEvent>
{
    public async Task Handle(TaskCommentedEvent notification, CancellationToken cancellationToken)
    {
        var comment = await commentRepository.GetByIdAsync(notification.CommentId);
        if (comment == null)
        {
            return;
        }

        await taskNotificationService.NotifyTaskCommented(
            notification.TaskId,
            notification.BoardId,
            notification.CommentId,
            notification.CommentAuthorId,
            comment.Content,
            cancellationToken);
    }
}

