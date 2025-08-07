using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Infrastructure.Persistence.Repositories;

public class ProjectRepository(ApplicationDbContext dbContext) : Repository<Project>(dbContext), IProjectRepository
{
    public override async Task AddAsync(Project project)
    {
        await base.AddAsync(project);
    }
    public override Task<Project?> GetByIdAsync(long id) => base.GetByIdAsync(id);
    
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
}