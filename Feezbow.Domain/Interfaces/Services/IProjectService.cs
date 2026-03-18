using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Domain.Interfaces.Services;

public interface IProjectService
{
    Task<Project> CreateProjectAsync(string? name, string? description, long creatorUserId, CancellationToken cancellationToken = default);

    Task<Project> UpdateProjectAsync(long id, string? name, string? description, ProjectStatus status,
        ProjectVisibility visibility, bool archived, long creatorUserId,  CancellationToken cancellationToken = default);
    Task<Project> GetProjectByIdAsync(long id, long currentUserId, CancellationToken cancellationToken = default);
    Task<long> ArchiveProjectAsync(long id, long currentUserId, CancellationToken cancellationToken = default);
    Task<long> AddProjectMember(long projectId, long newMemberId, ProjectRole role, long inviterUserId, CancellationToken cancellationToken = default);
    Task RemoveProjectMember(long projectId, long removedUserId, long removerUserId, CancellationToken cancellationToken = default);
    Task UpdateProjectMember(long projectId, long updateMemberId, ProjectRole role, long inviterUserId, CancellationToken cancellationToken = default);

}