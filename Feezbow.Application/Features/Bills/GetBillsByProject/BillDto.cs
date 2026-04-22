using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;

namespace Feezbow.Application.Features.Bills.GetBillsByProject;

public record BillSplitDto(
    long UserId,
    string? UserName,
    decimal Amount,
    bool IsPaid,
    DateTime? PaidAt);

public record BillRecurrenceDto(
    RecurrenceFrequency Frequency,
    int Interval,
    IReadOnlyList<DayOfWeek> DaysOfWeek,
    DateTime? EndDate);

public record BillDto(
    long Id,
    string Title,
    string? Description,
    string? Category,
    decimal Amount,
    string Currency,
    DateTime DueDate,
    bool IsPaid,
    DateTime? PaidAt,
    long? PaidBy,
    bool IsRecurring,
    DateTime? NextOccurrence,
    long? ParentBillId,
    BillRecurrenceDto? Recurrence,
    IReadOnlyList<BillSplitDto> Splits,
    DateTime CreatedAt)
{
    public static BillDto FromEntity(Bill bill) => new(
        bill.Id,
        bill.Title,
        bill.Description,
        bill.Category,
        bill.Amount,
        bill.Currency,
        bill.DueDate,
        bill.IsPaid,
        bill.PaidAt,
        bill.PaidBy,
        bill.IsRecurring,
        bill.NextOccurrence,
        bill.ParentBillId,
        bill.Recurrence is null ? null : new BillRecurrenceDto(
            bill.Recurrence.Frequency,
            bill.Recurrence.Interval,
            bill.Recurrence.GetDaysOfWeek(),
            bill.Recurrence.EndDate),
        bill.Splits.Select(s => new BillSplitDto(
            s.UserId,
            s.User is not null ? $"{s.User.FirstName} {s.User.LastName}".Trim() : null,
            s.Amount,
            s.IsPaid,
            s.PaidAt)).ToList(),
        bill.CreatedAt);
}
