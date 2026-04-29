using MediatR;
using Microsoft.Extensions.Logging;
using Feezbow.Domain.Enums;
using Feezbow.Domain.Events.Bill_Events;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Events.Bill;

/// <summary>
/// Cleans up orphaned attachments after a Bill is deleted. Failures are logged but not rethrown so
/// the deletion stays committed; a periodic sweep can mop up if individual file removals failed.
/// </summary>
public class BillDeletedEventHandler(
    IAttachmentService attachmentService,
    ILogger<BillDeletedEventHandler> logger) : INotificationHandler<BillDeletedEvent>
{
    public async Task Handle(BillDeletedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            await attachmentService.DeleteByOwnerAsync(
                AttachmentOwnerType.Bill,
                notification.BillId,
                notification.DeletedBy,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to clean up attachments for deleted bill {BillId} (project {ProjectId})",
                notification.BillId, notification.ProjectId);
        }
    }
}
