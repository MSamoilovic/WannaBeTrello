using Microsoft.EntityFrameworkCore;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Infrastructure.Persistence.Repositories;

public class ProjectRepository(ApplicationDbContext dbContext) : Repository<Project>(dbContext), IProjectRepository
{
    public Task<Project?> GetByIdAsync(long id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Project>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(Project project)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Project project)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(long id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Project>> GetProjectsByUserIdAsync(long userId)
    {
        throw new NotImplementedException();
    }

    public Task<List<ProjectMember>> GetProjectMembersByIdAsync(long projectId)
    {
        throw new NotImplementedException();
    }
}