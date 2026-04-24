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

    public async Task<IReadOnlyList<Bill>> GetRecurringBillsByProjectAsync(long projectId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(b => b.Splits)
            .ThenInclude(s => s.User)
            .Where(b => b.ProjectId == projectId && b.Recurrence != null)
            .OrderBy(b => b.NextOccurrence)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Bill>> GetRecurringBillsDueAsync(DateTime upTo, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(b => b.Splits)
            .Where(b => b.Recurrence != null
                     && b.NextOccurrence.HasValue
                     && b.NextOccurrence.Value.Date <= upTo.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Bill>> GetByProjectAndDateRangeAsync(
        long projectId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(b => b.Splits)
            .ThenInclude(s => s.User)
            .Where(b => b.ProjectId == projectId
                     && b.Recurrence == null
                     && b.DueDate >= from
                     && b.DueDate <= to)
            .OrderBy(b => b.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Bill>> GetByUserAndDateRangeAsync(
        long userId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(b => b.Project)
            .Include(b => b.Splits)
            .Where(b => b.Recurrence == null
                     && b.DueDate >= from
                     && b.DueDate <= to
                     && b.Splits.Any(s => s.UserId == userId))
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
