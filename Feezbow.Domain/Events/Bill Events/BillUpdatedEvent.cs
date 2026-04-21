namespace Feezbow.Domain.Events.Bill_Events;

public class BillUpdatedEvent(long billId, long projectId, long updatedBy) : DomainEvent
{
    public long BillId => billId;
    public long ProjectId => projectId;
    public long UpdatedBy => updatedBy;
}
