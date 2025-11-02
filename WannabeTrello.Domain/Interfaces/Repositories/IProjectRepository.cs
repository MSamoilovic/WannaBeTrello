using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Interfaces.Repositories;

public interface IProjectRepository: IRepository<Project>
{
    Task<Project?> GetProjectWithDetailsAsync(long projectId, CancellationToken cancellationToken = default);
    Task<Project?> GetProjectWithMembersAsync(long projectId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Project>> GetActiveProjectsByUserAsync(long userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProjectMember>> GetProjectMembersByProjectIdAsync(long projectId, CancellationToken cancellationToken = default);
    
    // Simple helper metode
    Task<bool> IsProjectMemberAsync(long projectId, long userId, CancellationToken cancellationToken = default);
}