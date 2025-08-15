using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Interfaces.Repositories;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(long id);
    Task<IEnumerable<Project>> GetAllAsync();
    Task AddAsync(Project project);
    Task UpdateAsync(Project project);
    Task DeleteAsync(long id);
    Task<IEnumerable<Project>> GetProjectsByUserIdAsync(long userId);
    Task<List<ProjectMember>> GetProjectMembersByIdAsync(long projectId);
    
}