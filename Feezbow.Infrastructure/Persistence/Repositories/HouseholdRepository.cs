using Microsoft.EntityFrameworkCore;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Infrastructure.Persistence.Repositories;

public class HouseholdRepository(ApplicationDbContext dbContext) : IHouseholdRepository
{
    private readonly DbSet<HouseholdProfile> _dbSet = dbContext.Set<HouseholdProfile>();

    public async Task<HouseholdProfile?> GetByProjectIdAsync(long projectId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(h => h.Project)
            .FirstOrDefaultAsync(h => h.ProjectId == projectId, cancellationToken);
    }

    public async Task AddAsync(HouseholdProfile profile, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(profile, cancellationToken);
    }

    public async Task<bool> ExistsForProjectAsync(long projectId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(h => h.ProjectId == projectId, cancellationToken);
    }
}
