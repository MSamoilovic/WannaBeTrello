using Feezbow.Domain.Exceptions;

namespace Feezbow.Domain.Entities;

public class BillSplit : AuditableEntity
{
    public long BillId { get; private set; }
    public long UserId { get; private set; }
    public User? User { get; private set; }
    public decimal Amount { get; private set; }
    public bool IsPaid { get; private set; }
    public DateTime? PaidAt { get; private set; }

    private BillSplit() { }

    internal static BillSplit Create(long billId, long userId, decimal amount, long createdBy)
    {
        if (userId <= 0)
            throw new BusinessRuleValidationException("UserId must be a positive number.");

        if (amount < 0)
            throw new BusinessRuleValidationException("Split amount cannot be negative.");

        if (createdBy <= 0)
            throw new BusinessRuleValidationException("CreatedBy must be a positive number.");

        return new BillSplit
        {
            BillId = billId,
            UserId = userId,
            Amount = amount,
            IsPaid = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    internal void MarkPaid(long paidBy)
    {
        if (paidBy <= 0)
            throw new BusinessRuleValidationException("PaidBy must be a positive number.");

        if (IsPaid) return;

        IsPaid = true;
        PaidAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = paidBy;
    }

    internal void MarkUnpaid(long updatedBy)
    {
        if (updatedBy <= 0)
            throw new BusinessRuleValidationException("UpdatedBy must be a positive number.");

        if (!IsPaid) return;

        IsPaid = false;
        PaidAt = null;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = updatedBy;
    }

    internal void UpdateAmount(decimal amount, long updatedBy)
    {
        if (amount < 0)
            throw new BusinessRuleValidationException("Split amount cannot be negative.");
        if (updatedBy <= 0)
            throw new BusinessRuleValidationException("UpdatedBy must be a positive number.");

        if (Amount == amount) return;

        Amount = amount;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = updatedBy;
    }
}
