using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Interfaces.Repositories;

public interface IHouseholdRepository
{
    Task<HouseholdProfile?> GetByProjectIdAsync(long projectId, CancellationToken cancellationToken = default);
    Task<HouseholdProfile?> GetByProjectIdWithMembersAsync(long projectId, CancellationToken cancellationToken = default);
    Task AddAsync(HouseholdProfile profile, CancellationToken cancellationToken = default);
    Task<bool> ExistsForProjectAsync(long projectId, CancellationToken cancellationToken = default);
}
