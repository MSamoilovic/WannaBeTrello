using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces;
using WannabeTrello.Domain.Interfaces.Repositories;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Domain.Services;

public class ProjectService(
    IProjectRepository projectRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IProjectService
{
    public async Task<Project> CreateProjectAsync(string? name, string? description, long creatorUserId,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(creatorUserId);
        if (user == null)
        {
            throw new NotFoundException(nameof(User), creatorUserId);
        }

        var project = Project.Create(name, description, user.Id);

        await projectRepository.AddAsync(project, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return project;
    }

    public async Task<Project> UpdateProjectAsync(long id, string? name, string? description, ProjectStatus status,
        ProjectVisibility visibility, bool archived, long creatorUserId, CancellationToken cancellationToken)
    {
        var project = await projectRepository.GetByIdAsync(id, cancellationToken);
        if (project is null)
            throw new NotFoundException(nameof(Project), id);

        var projectMember = project.ProjectMembers.SingleOrDefault(x =>
            x.UserId == creatorUserId && x.Role is ProjectRole.Admin or ProjectRole.Owner);

        if (projectMember is null) throw new AccessDeniedException("You don't have access to this project");

        project.Update(name, description, status, visibility, archived, creatorUserId);

        projectRepository.Update(project);
        await unitOfWork.CompleteAsync(cancellationToken);

        return project;
    }

    public async Task<Project> GetProjectByIdAsync(long id, long currentUserId, CancellationToken cancellationToken)
    {
        var project = await projectRepository.GetByIdAsync(id, cancellationToken);
        if (project is null)
            throw new NotFoundException(nameof(Project), id);

        return project.ProjectMembers.All(x => x.UserId != currentUserId)
            ? throw new AccessDeniedException("You don't have access to this project")
            : project;
    }

    public async Task<long> ArchiveProjectAsync(long id, long currentUserId, CancellationToken cancellationToken)
    {
        var project = await projectRepository.GetByIdAsync(id);
        if (project is null)
            throw new NotFoundException(nameof(Project), id);

        project.Archive(currentUserId);

        projectRepository.Update(project);
        await unitOfWork.CompleteAsync(cancellationToken);
        return project.Id;
    }

    //TODO: Dodaj za Unarchive funkcionalnost

    public async Task<long> AddProjectMember(long projectId, long newMemberId, ProjectRole role, long inviterUserId, CancellationToken cancellationToken)
    {
        var project = await projectRepository.GetByIdAsync(projectId, cancellationToken);
        if (project is null)
            throw new NotFoundException(nameof(Project), projectId);

        var newMember = await userRepository.GetByIdAsync(newMemberId);
        if (newMember is null)
            throw new NotFoundException(nameof(User), newMemberId);

        newMember.EnsureActive();

        project.AddMember(newMemberId, role, inviterUserId);

        projectRepository.Update(project);
        await unitOfWork.CompleteAsync(cancellationToken);

        return project.Id;
    }

    public async Task RemoveProjectMember(long projectId, long removedUserId, long removerUserId, CancellationToken cancellationToken)
    {
        var project = await projectRepository.GetByIdAsync(projectId, cancellationToken);
        if (project is null)
            throw new NotFoundException(nameof(Project), projectId);

        project.RemoveMember(removedUserId, removerUserId);

        projectRepository.Update(project);
        await unitOfWork.CompleteAsync(cancellationToken);
    }

    public async Task UpdateProjectMember(long projectId, long updateMemberId, ProjectRole role, long inviterUserId, CancellationToken cancellationToken)
    {
        var project = await projectRepository.GetByIdAsync(projectId);
        if (project is null)
            throw new NotFoundException(nameof(Project), projectId);

        project.UpdateMember(updateMemberId, role, inviterUserId);

        projectRepository.Update(project);
        await unitOfWork.CompleteAsync(cancellationToken);
    }
}