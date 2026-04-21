using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Interfaces.Repositories;

public interface IBillRepository
{
    Task<Bill?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Bill>> GetByProjectAsync(long projectId, bool includePaid, CancellationToken cancellationToken = default);
    Task AddAsync(Bill bill, CancellationToken cancellationToken = default);
    void Remove(Bill bill);
}
