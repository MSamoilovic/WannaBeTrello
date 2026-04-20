using Microsoft.EntityFrameworkCore;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Infrastructure.Persistence.Repositories;

public class BillRepository(ApplicationDbContext dbContext) : IBillRepository
{
    private readonly DbSet<Bill> _dbSet = dbContext.Set<Bill>();

    public async Task<Bill?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(b => b.Project)
            .ThenInclude(p => p.ProjectMembers)
            .Include(b => b.Splits)
            .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Bill>> GetByProjectAsync(long projectId, bool includePaid, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(b => b.Splits)
            .ThenInclude(s => s.User)
            .Where(b => b.ProjectId == projectId);

        if (!includePaid)
            query = query.Where(b => !b.IsPaid);

        return await query
            .OrderBy(b => b.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Bill bill, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(bill, cancellationToken);
    }

    public void Remove(Bill bill)
    {
        _dbSet.Remove(bill);
    }
}
