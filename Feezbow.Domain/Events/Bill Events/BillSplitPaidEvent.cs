namespace Feezbow.Domain.Events.Bill_Events;

public class BillSplitPaidEvent(long billId, long projectId, long userId, decimal amount, long paidBy) : DomainEvent
{
    public long BillId => billId;
    public long ProjectId => projectId;
    public long UserId => userId;
    public decimal Amount => amount;
    public long PaidBy => paidBy;
}
