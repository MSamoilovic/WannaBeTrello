using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Interfaces.Repositories;

public interface IBillRepository
{
    Task<Bill?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Bill>> GetByProjectAsync(long projectId, bool includePaid, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Bill>> GetRecurringBillsByProjectAsync(long projectId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Bill>> GetRecurringBillsDueAsync(DateTime upTo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns non-template bills (ParentBillId set, or Recurrence null) for a project whose DueDate falls within [from, to].
    /// Used by budget aggregations.
    /// </summary>
    Task<IReadOnlyList<Bill>> GetByProjectAndDateRangeAsync(
        long projectId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns non-template bills across all projects the user belongs to, within [from, to].
    /// Used by user-level budget aggregations.
    /// </summary>
    Task<IReadOnlyList<Bill>> GetByUserAndDateRangeAsync(
        long userId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);

    Task AddAsync(Bill bill, CancellationToken cancellationToken = default);
    void Remove(Bill bill);
}
