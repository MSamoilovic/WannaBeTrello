using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Events.TaskEvents;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Application.Features.Events.BoardTask;

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
            notification.CommentId,
            notification.CommentAuthorId,
            comment.Content,
            cancellationToken);
    }
}

