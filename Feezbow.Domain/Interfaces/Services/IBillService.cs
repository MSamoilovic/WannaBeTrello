using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;

namespace Feezbow.Domain.Interfaces.Services;

public interface IBillService
{
    Task<Bill> CreateBillAsync(
        long projectId,
        long userId,
        string title,
        decimal amount,
        DateTime dueDate,
        string currency,
        string? description,
        string? category,
        IReadOnlyCollection<long>? splitUserIds,
        RecurrenceFrequency? recurrenceFrequency,
        int recurrenceInterval,
        IEnumerable<DayOfWeek>? recurrenceDaysOfWeek,
        DateTime? recurrenceEndDate,
        CancellationToken cancellationToken = default);

    Task<long> UpdateBillAsync(
        long billId,
        long userId,
        string? title,
        string? description,
        string? category,
        decimal? amount,
        DateTime? dueDate,
        CancellationToken cancellationToken = default);

    Task<long> DeleteBillAsync(
        long billId,
        long userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Bill>> GetByProjectAsync(
        long projectId,
        long userId,
        bool includePaid,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Bill>> GetRecurringBillsAsync(
        long projectId,
        long userId,
        CancellationToken cancellationToken = default);

    Task<long> MarkBillPaidAsync(
        long billId,
        long userId,
        CancellationToken cancellationToken = default);

    Task<long> UpdateBillRecurrenceAsync(
        long billId,
        long userId,
        RecurrenceFrequency frequency,
        int interval,
        IEnumerable<DayOfWeek>? daysOfWeek,
        DateTime? endDate,
        CancellationToken cancellationToken = default);

    Task<long> CancelBillRecurrenceAsync(
        long billId,
        long userId,
        CancellationToken cancellationToken = default);

    Task<long> RecordSplitPaymentAsync(
        long billId,
        long splitUserId,
        long userId,
        CancellationToken cancellationToken = default);

    Task<long> SetBillSplitAsync(
        long billId,
        long userId,
        IReadOnlyCollection<long>? equalSplitUserIds,
        IReadOnlyCollection<(long UserId, decimal Amount)>? customShares,
        CancellationToken cancellationToken = default);
}
