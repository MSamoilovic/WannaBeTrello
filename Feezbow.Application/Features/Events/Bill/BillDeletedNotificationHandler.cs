using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Bill_Events;

namespace Feezbow.Application.Features.Events.Bill;

public class BillDeletedNotificationHandler(IBillNotificationService notifications)
    : INotificationHandler<BillDeletedEvent>
{
    public Task Handle(BillDeletedEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyBillDeleted(
            notification.BillId,
            notification.ProjectId,
            notification.DeletedBy,
            cancellationToken);
}
