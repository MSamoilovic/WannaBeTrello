using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Attachment_Events;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Features.Events.Attachment;

public class AttachmentDeletedEventHandler(
    IAttachmentNotificationService notifications,
    IBillRepository billRepository) : INotificationHandler<AttachmentDeletedEvent>
{
    public async Task Handle(AttachmentDeletedEvent notification, CancellationToken cancellationToken)
    {
        var projectId = await AttachmentProjectResolver.ResolveAsync(
            notification.OwnerType, notification.OwnerId, billRepository, cancellationToken);

        if (projectId is null) return;

        await notifications.NotifyDeleted(
            notification.AttachmentId,
            projectId.Value,
            notification.OwnerType,
            notification.OwnerId,
            notification.DeletedBy,
            cancellationToken);
    }
}
