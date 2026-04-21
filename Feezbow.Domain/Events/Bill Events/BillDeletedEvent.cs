namespace Feezbow.Domain.Events.Bill_Events;

public class BillDeletedEvent(long billId, long projectId, long deletedBy) : DomainEvent
{
    public long BillId => billId;
    public long ProjectId => projectId;
    public long DeletedBy => deletedBy;
}
