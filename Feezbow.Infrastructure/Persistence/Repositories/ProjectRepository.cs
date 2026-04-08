using Microsoft.EntityFrameworkCore;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Interfaces.Repositories;
using Feezbow.Domain.Specifications.ProjectSpecifications;

namespace Feezbow.Infrastructure.Persistence.Repositories;

public class ProjectRepository(ApplicationDbContext dbContext) : Repository<Project>(dbContext), IProjectRepository
{
    public async Task<Project?> GetProjectWithDetailsAsync(long projectId, CancellationToken cancellationToken = default)
    {
        var spec = new ProjectWithDetailsSpecification(projectId);
        return await GetSingleAsync(spec, cancellationToken);
    }

    public async Task<Project?> GetProjectWithMembersAsync(long projectId, CancellationToken cancellationToken = default)
    {
        var spec = new ProjectWithMembersSpecification(projectId);
        return await GetSingleAsync(spec, cancellationToken);
    }

    public async Task<Project?> GetArchivedProjectWithMembersAsync(long projectId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .IgnoreQueryFilters()
            .Include(p => p.ProjectMembers)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> GetActiveProjectsByUserAsync(long userId, CancellationToken cancellationToken = default)
    {
        var spec = new ActiveProjectsByUserSpecification(userId);
        return await GetAsync(spec, cancellationToken);
    }

    public async Task<IReadOnlyList<ProjectMember>> GetProjectMembersByProjectIdAsync(long projectId, CancellationToken cancellationToken = default)
    {
        var spec = new ProjectMembersByProjectIdSpecification(projectId);
        var query = SpecificationQueryBuilder.GetQuery(_dbContext.ProjectMembers, spec);
        return await query.ToListAsync(cancellationToken);
    }

    
    public async Task<bool> IsProjectMemberAsync(long projectId, long userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId, cancellationToken);
    }
}