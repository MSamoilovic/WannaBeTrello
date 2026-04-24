using Feezbow.Domain.Events.Bill_Events;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Services;
using Feezbow.Domain.ValueObjects;

namespace Feezbow.Domain.Entities;

public class Bill : AuditableEntity
{
    public long ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? Category { get; private set; }

    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "EUR";

    public DateTime DueDate { get; private set; }
    public bool IsPaid { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public long? PaidBy { get; private set; }

    public bool IsRecurring { get; private set; }
    public RecurrenceRule? Recurrence { get; private set; }

    /// <summary>When the next occurrence should be spawned by the generator job. Null if not recurring or expired.</summary>
    public DateTime? NextOccurrence { get; private set; }

    /// <summary>ID of the recurring template bill this was spawned from. Null for templates / one-off bills.</summary>
    public long? ParentBillId { get; private set; }
    public Bill? ParentBill { get; private set; }

    private readonly List<BillSplit> _splits = [];
    public IReadOnlyCollection<BillSplit> Splits => _splits.AsReadOnly();

    private Bill() { }

    public static Bill Create(
        long projectId,
        string title,
        decimal amount,
        DateTime dueDate,
        long createdBy,
        string currency = "EUR",
        string? description = null,
        string? category = null,
        RecurrenceRule? recurrence = null)
    {
        if (projectId <= 0)
            throw new BusinessRuleValidationException("ProjectId must be a positive number.");

        if (string.IsNullOrWhiteSpace(title))
            throw new BusinessRuleValidationException("Bill title cannot be empty.");

        if (amount <= 0)
            throw new BusinessRuleValidationException("Bill amount must be greater than zero.");

        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
            throw new BusinessRuleValidationException("Currency must be a 3-letter ISO code.");

        if (createdBy <= 0)
            throw new BusinessRuleValidationException("CreatedBy must be a positive number.");

        if (dueDate.Date < DateTime.UtcNow.Date)
            throw new BusinessRuleValidationException("Due date cannot be in the past.");

        var bill = new Bill
        {
            ProjectId = projectId,
            Title = title.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim(),
            Amount = amount,
            Currency = currency.Trim().ToUpperInvariant(),
            DueDate = dueDate,
            IsPaid = false,
            IsRecurring = recurrence is not null,
            Recurrence = recurrence,
            NextOccurrence = recurrence is null ? null : RecurringTaskScheduler.CalculateNext(dueDate, recurrence),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        bill.AddDomainEvent(new BillCreatedEvent(bill.Id, projectId, bill.Title, amount, bill.Currency, createdBy));
        return bill;
    }

    public void Update(string? title, string? description, string? category, decimal? amount,
        DateTime? dueDate, long updatedBy)
    {
        if (updatedBy <= 0)
            throw new BusinessRuleValidationException("UpdatedBy must be a positive number.");

        if (IsPaid)
            throw new InvalidOperationDomainException("Cannot update a bill that has already been paid.");

        var changed = false;

        if (!string.IsNullOrWhiteSpace(title))
        {
            var normalized = title.Trim();
            if (!string.Equals(Title, normalized, StringComparison.Ordinal))
            {
                Title = normalized;
                changed = true;
            }
        }

        if (description is not null)
        {
            var normalized = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            if (!string.Equals(Description, normalized, StringComparison.Ordinal))
            {
                Description = normalized;
                changed = true;
            }
        }

        if (category is not null)
        {
            var normalized = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
            if (!string.Equals(Category, normalized, StringComparison.Ordinal))
            {
                Category = normalized;
                changed = true;
            }
        }

        if (amount.HasValue)
        {
            if (amount.Value <= 0)
                throw new BusinessRuleValidationException("Bill amount must be greater than zero.");

            if (Amount != amount.Value)
            {
                if (_splits.Count > 0)
                    throw new InvalidOperationDomainException("Cannot change amount after splits have been assigned. Clear splits first.");

                Amount = amount.Value;
                changed = true;
            }
        }

        if (dueDate.HasValue)
        {
            if (dueDate.Value.Date < DateTime.UtcNow.Date)
                throw new BusinessRuleValidationException("Due date cannot be in the past.");

            if (DueDate != dueDate.Value)
            {
                DueDate = dueDate.Value;
                changed = true;
            }
        }

        if (!changed) return;

        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = updatedBy;

        AddDomainEvent(new BillUpdatedEvent(Id, ProjectId, updatedBy));
    }

    public void SetEqualSplit(IReadOnlyCollection<long> userIds, long updatedBy)
    {
        if (updatedBy <= 0)
            throw new BusinessRuleValidationException("UpdatedBy must be a positive number.");

        if (userIds is null || userIds.Count == 0)
            throw new BusinessRuleValidationException("At least one user is required to split a bill.");

        if (IsPaid)
            throw new InvalidOperationDomainException("Cannot modify splits on a paid bill.");

        var distinctIds = userIds.Distinct().ToList();
        var share = Math.Round(Amount / distinctIds.Count, 2, MidpointRounding.ToEven);
        var remainder = Amount - share * distinctIds.Count;

        _splits.Clear();
        for (var i = 0; i < distinctIds.Count; i++)
        {
            var amount = i == 0 ? share + remainder : share;
            _splits.Add(BillSplit.Create(Id, distinctIds[i], amount, updatedBy));
        }

        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = updatedBy;
    }

    public void SetCustomSplit(IReadOnlyCollection<(long userId, decimal amount)> shares, long updatedBy)
    {
        if (updatedBy <= 0)
            throw new BusinessRuleValidationException("UpdatedBy must be a positive number.");

        if (shares is null || shares.Count == 0)
            throw new BusinessRuleValidationException("At least one share is required.");

        if (IsPaid)
            throw new InvalidOperationDomainException("Cannot modify splits on a paid bill.");

        var total = shares.Sum(s => s.amount);
        if (Math.Abs(total - Amount) > 0.01m)
            throw new BusinessRuleValidationException("Sum of splits must equal bill amount.");

        _splits.Clear();
        foreach (var (userId, amount) in shares)
        {
            _splits.Add(BillSplit.Create(Id, userId, amount, updatedBy));
        }

        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = updatedBy;
    }

    public void RecordSplitPayment(long userId, long paidBy)
    {
        if (paidBy <= 0)
            throw new BusinessRuleValidationException("PaidBy must be a positive number.");

        var split = _splits.FirstOrDefault(s => s.UserId == userId)
            ?? throw new NotFoundException(nameof(BillSplit), userId);

        if (split.IsPaid) return;

        split.MarkPaid(paidBy);
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = paidBy;

        AddDomainEvent(new BillSplitPaidEvent(Id, ProjectId, userId, split.Amount, paidBy));

        if (_splits.All(s => s.IsPaid))
            MarkPaidInternal(paidBy);
    }

    public void MarkFullyPaid(long paidBy)
    {
        if (paidBy <= 0)
            throw new BusinessRuleValidationException("PaidBy must be a positive number.");

        if (IsPaid) return;

        foreach (var split in _splits)
            split.MarkPaid(paidBy);

        MarkPaidInternal(paidBy);
    }

    private void MarkPaidInternal(long paidBy)
    {
        IsPaid = true;
        PaidAt = DateTime.UtcNow;
        PaidBy = paidBy;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = paidBy;

        AddDomainEvent(new BillPaidEvent(Id, ProjectId, Amount, paidBy));
    }

    public void SetRecurrence(RecurrenceRule rule, DateTime? firstOccurrence, long modifierUserId)
    {
        if (modifierUserId <= 0)
            throw new BusinessRuleValidationException("ModifierUserId must be a positive number.");

        Recurrence = rule ?? throw new ArgumentNullException(nameof(rule));
        IsRecurring = true;
        NextOccurrence = firstOccurrence ?? RecurringTaskScheduler.CalculateNext(DueDate, rule);
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifierUserId;
    }

    public void ClearRecurrence(long modifierUserId)
    {
        if (modifierUserId <= 0)
            throw new BusinessRuleValidationException("ModifierUserId must be a positive number.");

        if (Recurrence is null) return;

        Recurrence = null;
        IsRecurring = false;
        NextOccurrence = null;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifierUserId;
    }

    public void ScheduleNextOccurrence(DateTime nextDate, long systemUserId)
    {
        NextOccurrence = nextDate;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = systemUserId;
    }

    /// <summary>
    /// Creates a fresh, unpaid Bill instance based on this recurring template.
    /// Splits are copied (same users, same amounts, all unpaid). The new bill carries
    /// no recurrence of its own; ParentBillId points to this template.
    /// </summary>
    public Bill SpawnOccurrence(long systemUserId)
    {
        if (Recurrence is null || !NextOccurrence.HasValue)
            throw new InvalidOperationDomainException("Cannot spawn occurrence from a non-recurring bill.");

        var occurrence = new Bill
        {
            ProjectId = ProjectId,
            Title = Title,
            Description = Description,
            Category = Category,
            Amount = Amount,
            Currency = Currency,
            DueDate = NextOccurrence.Value,
            IsPaid = false,
            IsRecurring = false,
            Recurrence = null,
            NextOccurrence = null,
            ParentBillId = Id,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = systemUserId
        };

        foreach (var split in _splits)
            occurrence._splits.Add(BillSplit.CopyForSpawn(split.UserId, split.Amount, systemUserId));

        occurrence.AddDomainEvent(new BillCreatedEvent(
            occurrence.Id, occurrence.ProjectId, occurrence.Title, occurrence.Amount, occurrence.Currency, systemUserId));

        return occurrence;
    }
}
