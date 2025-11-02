using Microsoft.EntityFrameworkCore;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces.Repositories;
using WannabeTrello.Domain.Specifications.ProjectSpecifications;

namespace WannabeTrello.Infrastructure.Persistence.Repositories;

public class ProjectRepository: Repository<Project>, IProjectRepository
{
    public ProjectRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
    
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