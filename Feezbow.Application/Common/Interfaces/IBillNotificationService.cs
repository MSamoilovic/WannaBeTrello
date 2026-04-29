namespace Feezbow.Application.Common.Interfaces;

public interface IBillNotificationService
{
    Task NotifyBillCreated(long billId, long projectId, string title, decimal amount, string currency, long createdBy, CancellationToken cancellationToken = default);
    Task NotifyBillUpdated(long billId, long projectId, long modifiedBy, CancellationToken cancellationToken = default);
    Task NotifyBillPaid(long billId, long projectId, decimal amount, long paidBy, CancellationToken cancellationToken = default);
    Task NotifyBillSplitPaid(long billId, long projectId, long userId, decimal amount, long paidBy, CancellationToken cancellationToken = default);
    Task NotifyBillDeleted(long billId, long projectId, long deletedBy, CancellationToken cancellationToken = default);
}
