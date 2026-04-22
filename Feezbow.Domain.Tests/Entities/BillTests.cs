using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;
using Feezbow.Domain.Events.Bill_Events;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.ValueObjects;

namespace Feezbow.Domain.Tests.Entities;

public class BillTests
{
    private const long ProjectId = 10L;
    private const long UserId = 5L;

    private static DateTime FutureDueDate => DateTime.UtcNow.Date.AddDays(5);

    [Fact]
    [Trait("Category", "Domain")]
    public void Create_ValidArguments_ReturnsInitializedBill()
    {
        var bill = Bill.Create(ProjectId, "Electricity", 120m, FutureDueDate, UserId);

        Assert.Equal(ProjectId, bill.ProjectId);
        Assert.Equal("Electricity", bill.Title);
        Assert.Equal(120m, bill.Amount);
        Assert.Equal("EUR", bill.Currency);
        Assert.False(bill.IsPaid);
        Assert.False(bill.IsRecurring);
        Assert.Empty(bill.Splits);
        Assert.Single(bill.DomainEvents);
        Assert.IsType<BillCreatedEvent>(bill.DomainEvents.First());
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Create_WithRecurrence_SetsIsRecurring()
    {
        var rule = RecurrenceRule.Create(RecurrenceFrequency.Monthly);

        var bill = Bill.Create(ProjectId, "Rent", 500m, FutureDueDate, UserId,
            description: "Monthly rent", category: "Housing", recurrence: rule);

        Assert.True(bill.IsRecurring);
        Assert.NotNull(bill.Recurrence);
        Assert.Equal("Housing", bill.Category);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Create_NonPositiveAmount_Throws()
    {
        Assert.Throws<BusinessRuleValidationException>(() =>
            Bill.Create(ProjectId, "Title", 0m, FutureDueDate, UserId));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Create_EmptyTitle_Throws()
    {
        Assert.Throws<BusinessRuleValidationException>(() =>
            Bill.Create(ProjectId, "", 100m, FutureDueDate, UserId));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Create_InvalidCurrency_Throws()
    {
        Assert.Throws<BusinessRuleValidationException>(() =>
            Bill.Create(ProjectId, "Title", 100m, FutureDueDate, UserId, currency: "EURO"));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Create_PastDueDate_Throws()
    {
        Assert.Throws<BusinessRuleValidationException>(() =>
            Bill.Create(ProjectId, "Title", 100m, DateTime.UtcNow.Date.AddDays(-1), UserId));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Update_ChangesTitle_RaisesEvent()
    {
        var bill = Bill.Create(ProjectId, "Old", 100m, FutureDueDate, UserId);
        bill.ClearDomainEvents();

        bill.Update("New", null, null, null, null, UserId);

        Assert.Equal("New", bill.Title);
        Assert.Single(bill.DomainEvents);
        Assert.IsType<BillUpdatedEvent>(bill.DomainEvents.First());
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Update_WhenAlreadyPaid_Throws()
    {
        var bill = Bill.Create(ProjectId, "Title", 100m, FutureDueDate, UserId);
        bill.MarkFullyPaid(UserId);

        Assert.Throws<InvalidOperationDomainException>(() =>
            bill.Update("New", null, null, null, null, UserId));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Update_AmountWithExistingSplits_Throws()
    {
        var bill = Bill.Create(ProjectId, "Title", 100m, FutureDueDate, UserId);
        bill.SetEqualSplit(new[] { 1L, 2L }, UserId);

        Assert.Throws<InvalidOperationDomainException>(() =>
            bill.Update(null, null, null, 200m, null, UserId));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void SetEqualSplit_DividesAmountAcrossUsers()
    {
        var bill = Bill.Create(ProjectId, "Title", 100m, FutureDueDate, UserId);

        bill.SetEqualSplit(new[] { 1L, 2L, 3L, 4L }, UserId);

        Assert.Equal(4, bill.Splits.Count);
        Assert.Equal(100m, bill.Splits.Sum(s => s.Amount));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void SetEqualSplit_EmptyList_Throws()
    {
        var bill = Bill.Create(ProjectId, "Title", 100m, FutureDueDate, UserId);

        Assert.Throws<BusinessRuleValidationException>(() =>
            bill.SetEqualSplit(Array.Empty<long>(), UserId));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void SetEqualSplit_WhenPaid_Throws()
    {
        var bill = Bill.Create(ProjectId, "Title", 100m, FutureDueDate, UserId);
        bill.MarkFullyPaid(UserId);

        Assert.Throws<InvalidOperationDomainException>(() =>
            bill.SetEqualSplit(new[] { 1L, 2L }, UserId));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void SetCustomSplit_ValidShares_SetsSplits()
    {
        var bill = Bill.Create(ProjectId, "Title", 100m, FutureDueDate, UserId);

        bill.SetCustomSplit(new[] { (1L, 70m), (2L, 30m) }, UserId);

        Assert.Equal(2, bill.Splits.Count);
        Assert.Equal(70m, bill.Splits.First(s => s.UserId == 1L).Amount);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void SetCustomSplit_SumMismatch_Throws()
    {
        var bill = Bill.Create(ProjectId, "Title", 100m, FutureDueDate, UserId);

        Assert.Throws<BusinessRuleValidationException>(() =>
            bill.SetCustomSplit(new[] { (1L, 50m), (2L, 30m) }, UserId));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void RecordSplitPayment_KnownUser_MarksSplitPaid()
    {
        var bill = Bill.Create(ProjectId, "Title", 100m, FutureDueDate, UserId);
        bill.SetEqualSplit(new[] { 1L, 2L }, UserId);
        bill.ClearDomainEvents();

        bill.RecordSplitPayment(1L, UserId);

        var split = bill.Splits.First(s => s.UserId == 1L);
        Assert.True(split.IsPaid);
        Assert.False(bill.IsPaid);
        Assert.Single(bill.DomainEvents);
        Assert.IsType<BillSplitPaidEvent>(bill.DomainEvents.First());
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void RecordSplitPayment_UnknownUser_Throws()
    {
        var bill = Bill.Create(ProjectId, "Title", 100m, FutureDueDate, UserId);
        bill.SetEqualSplit(new[] { 1L, 2L }, UserId);

        Assert.Throws<NotFoundException>(() =>
            bill.RecordSplitPayment(999L, UserId));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void RecordSplitPayment_AllSplitsPaid_MarksBillPaid()
    {
        var bill = Bill.Create(ProjectId, "Title", 100m, FutureDueDate, UserId);
        bill.SetEqualSplit(new[] { 1L, 2L }, UserId);
        bill.ClearDomainEvents();

        bill.RecordSplitPayment(1L, UserId);
        bill.RecordSplitPayment(2L, UserId);

        Assert.True(bill.IsPaid);
        Assert.Contains(bill.DomainEvents, e => e is BillPaidEvent);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void MarkFullyPaid_NotPaid_SetsPaidAndMarksAllSplits()
    {
        var bill = Bill.Create(ProjectId, "Title", 100m, FutureDueDate, UserId);
        bill.SetEqualSplit(new[] { 1L, 2L }, UserId);
        bill.ClearDomainEvents();

        bill.MarkFullyPaid(UserId);

        Assert.True(bill.IsPaid);
        Assert.NotNull(bill.PaidAt);
        Assert.All(bill.Splits, s => Assert.True(s.IsPaid));
        Assert.Contains(bill.DomainEvents, e => e is BillPaidEvent);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void MarkFullyPaid_AlreadyPaid_IsNoOp()
    {
        var bill = Bill.Create(ProjectId, "Title", 100m, FutureDueDate, UserId);
        bill.MarkFullyPaid(UserId);
        bill.ClearDomainEvents();

        bill.MarkFullyPaid(UserId);

        Assert.Empty(bill.DomainEvents);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void MarkFullyPaid_RecurringBill_DoesNotSpawnNext()
    {
        var rule = RecurrenceRule.Create(RecurrenceFrequency.Monthly);
        var dueDate = DateTime.UtcNow.Date.AddDays(1);
        var bill = Bill.Create(ProjectId, "Internet", 50m, dueDate, UserId, recurrence: rule);

        bill.MarkFullyPaid(UserId);

        Assert.True(bill.IsPaid);
        Assert.True(bill.IsRecurring);
        Assert.Equal(dueDate.AddMonths(1), bill.NextOccurrence);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void MarkFullyPaid_NonPositivePaidBy_Throws()
    {
        var bill = Bill.Create(ProjectId, "Title", 100m, FutureDueDate, UserId);

        Assert.Throws<BusinessRuleValidationException>(() => bill.MarkFullyPaid(0));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Create_Recurring_SetsNextOccurrenceFromRule()
    {
        var rule = RecurrenceRule.Create(RecurrenceFrequency.Monthly);
        var dueDate = DateTime.UtcNow.Date.AddDays(3);

        var bill = Bill.Create(ProjectId, "Rent", 500m, dueDate, UserId, recurrence: rule);

        Assert.True(bill.IsRecurring);
        Assert.Equal(dueDate.AddMonths(1), bill.NextOccurrence);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Create_NonRecurring_LeavesNextOccurrenceNull()
    {
        var bill = Bill.Create(ProjectId, "One-off", 20m, FutureDueDate, UserId);

        Assert.False(bill.IsRecurring);
        Assert.Null(bill.NextOccurrence);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void SetRecurrence_OnNonRecurringBill_MakesItRecurring()
    {
        var bill = Bill.Create(ProjectId, "Title", 100m, FutureDueDate, UserId);
        var rule = RecurrenceRule.Create(RecurrenceFrequency.Weekly);

        bill.SetRecurrence(rule, firstOccurrence: null, UserId);

        Assert.True(bill.IsRecurring);
        Assert.NotNull(bill.Recurrence);
        Assert.NotNull(bill.NextOccurrence);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void SetRecurrence_WithExplicitFirstOccurrence_UsesIt()
    {
        var bill = Bill.Create(ProjectId, "Title", 100m, FutureDueDate, UserId);
        var rule = RecurrenceRule.Create(RecurrenceFrequency.Monthly);
        var first = DateTime.UtcNow.Date.AddDays(14);

        bill.SetRecurrence(rule, firstOccurrence: first, UserId);

        Assert.Equal(first, bill.NextOccurrence);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void ClearRecurrence_RemovesRuleAndNextOccurrence()
    {
        var rule = RecurrenceRule.Create(RecurrenceFrequency.Monthly);
        var bill = Bill.Create(ProjectId, "Rent", 500m, FutureDueDate, UserId, recurrence: rule);

        bill.ClearRecurrence(UserId);

        Assert.False(bill.IsRecurring);
        Assert.Null(bill.Recurrence);
        Assert.Null(bill.NextOccurrence);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void ClearRecurrence_WhenAlreadyNonRecurring_IsNoOp()
    {
        var bill = Bill.Create(ProjectId, "Title", 100m, FutureDueDate, UserId);

        bill.ClearRecurrence(UserId);

        Assert.False(bill.IsRecurring);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void ScheduleNextOccurrence_UpdatesNextOccurrence()
    {
        var rule = RecurrenceRule.Create(RecurrenceFrequency.Monthly);
        var bill = Bill.Create(ProjectId, "Rent", 500m, FutureDueDate, UserId, recurrence: rule);
        var newNext = DateTime.UtcNow.Date.AddMonths(3);

        bill.ScheduleNextOccurrence(newNext, systemUserId: 0);

        Assert.Equal(newNext, bill.NextOccurrence);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void SpawnOccurrence_NonRecurring_Throws()
    {
        var bill = Bill.Create(ProjectId, "Title", 100m, FutureDueDate, UserId);

        Assert.Throws<InvalidOperationDomainException>(() => bill.SpawnOccurrence(systemUserId: 0));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void SpawnOccurrence_Recurring_CopiesFieldsAndSplits()
    {
        var rule = RecurrenceRule.Create(RecurrenceFrequency.Monthly);
        var dueDate = DateTime.UtcNow.Date.AddDays(1);
        var template = Bill.Create(ProjectId, "Rent", 500m, dueDate, UserId,
            description: "Monthly rent", category: "Housing", recurrence: rule);
        template.SetEqualSplit(new[] { 1L, 2L }, UserId);

        var occurrence = template.SpawnOccurrence(systemUserId: 0);

        Assert.Equal(template.ProjectId, occurrence.ProjectId);
        Assert.Equal(template.Title, occurrence.Title);
        Assert.Equal(template.Description, occurrence.Description);
        Assert.Equal(template.Category, occurrence.Category);
        Assert.Equal(template.Amount, occurrence.Amount);
        Assert.Equal(template.Currency, occurrence.Currency);
        Assert.Equal(template.NextOccurrence, occurrence.DueDate);
        Assert.Equal(template.Id, occurrence.ParentBillId);
        Assert.False(occurrence.IsRecurring);
        Assert.Null(occurrence.Recurrence);
        Assert.Null(occurrence.NextOccurrence);
        Assert.False(occurrence.IsPaid);
        Assert.Equal(2, occurrence.Splits.Count);
        Assert.All(occurrence.Splits, s => Assert.False(s.IsPaid));
        Assert.Equal(template.Splits.Sum(s => s.Amount), occurrence.Splits.Sum(s => s.Amount));
    }
}
