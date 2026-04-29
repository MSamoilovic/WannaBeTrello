using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Bill_Events;

namespace Feezbow.Application.Features.Events.Bill;

public class BillUpdatedEventHandler(IBillNotificationService notifications)
    : INotificationHandler<BillUpdatedEvent>
{
    public Task Handle(BillUpdatedEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyBillUpdated(
            notification.BillId,
            notification.ProjectId,
            notification.UpdatedBy,
            cancellationToken);
}
