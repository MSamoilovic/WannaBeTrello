using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Attachment_Events;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Features.Events.Attachment;

public class AttachmentUploadedEventHandler(
    IAttachmentNotificationService notifications,
    IBillRepository billRepository) : INotificationHandler<AttachmentUploadedEvent>
{
    public async Task Handle(AttachmentUploadedEvent notification, CancellationToken cancellationToken)
    {
        var projectId = await AttachmentProjectResolver.ResolveAsync(
            notification.OwnerType, notification.OwnerId, billRepository, cancellationToken);

        if (projectId is null) return;

        await notifications.NotifyUploaded(
            notification.AttachmentId,
            projectId.Value,
            notification.OwnerType,
            notification.OwnerId,
            notification.FileName,
            notification.SizeBytes,
            notification.UploadedBy,
            cancellationToken);
    }
}
