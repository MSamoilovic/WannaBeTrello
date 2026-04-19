using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Interfaces.Repositories;

public interface IHouseholdChoreRepository
{
    Task<HouseholdChore?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HouseholdChore>> GetByProjectAsync(long projectId, bool includeCompleted, CancellationToken cancellationToken = default);
    Task AddAsync(HouseholdChore chore, CancellationToken cancellationToken = default);
    void Remove(HouseholdChore chore);
}
