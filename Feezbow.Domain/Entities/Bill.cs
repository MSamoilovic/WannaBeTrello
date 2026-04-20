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

    public Bill? MarkFullyPaid(long paidBy)
    {
        if (paidBy <= 0)
            throw new BusinessRuleValidationException("PaidBy must be a positive number.");

        if (IsPaid) return null;

        foreach (var split in _splits)
            split.MarkPaid(paidBy);

        return MarkPaidInternal(paidBy);
    }

    private Bill? MarkPaidInternal(long paidBy)
    {
        IsPaid = true;
        PaidAt = DateTime.UtcNow;
        PaidBy = paidBy;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = paidBy;

        AddDomainEvent(new BillPaidEvent(Id, ProjectId, Amount, paidBy));

        if (!IsRecurring || Recurrence is null) return null;

        var nextDue = RecurringTaskScheduler.CalculateNext(DueDate, Recurrence);
        if (nextDue is null) return null;

        return Create(ProjectId, Title, Amount, nextDue.Value, paidBy, Currency, Description, Category, Recurrence);
    }
}
