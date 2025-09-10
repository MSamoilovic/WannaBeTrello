using Microsoft.EntityFrameworkCore;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Infrastructure.Persistence.Repositories;

public class ProjectRepository(ApplicationDbContext dbContext) : Repository<Project>(dbContext), IProjectRepository
{
    public override async Task AddAsync(Project project)
    {
        await base.AddAsync(project);
    }

    public override async Task<Project?> GetByIdAsync(long id)
    {
        return await _dbSet.Include(p => p.ProjectMembers)
            .SingleOrDefaultAsync(p => p.Id == id);
    }

    public async Task UpdateAsync(Project project)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteAsync(long id)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Project>> GetProjectsByUserIdAsync(long userId)
    {
        throw new NotImplementedException();
    }

    public async Task<List<ProjectMember>> GetProjectMembersByIdAsync(long projectId)
    {
        return await _dbSet
            .Where(p => p.Id == projectId)
            .Include(p => p.ProjectMembers)
            .ThenInclude(pm => pm.User)
            .SelectMany(p => p.ProjectMembers)
            .ToListAsync();
    }
}