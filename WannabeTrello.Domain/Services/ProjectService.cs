using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Domain.Services;

public class ProjectService(
    IProjectRepository projectRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<Project> CreateProjectAsync(string? name, string? description, long creatorUserId)
    {
        var user = await userRepository.GetByIdAsync(creatorUserId);
        if (user == null)
        {
            throw new NotFoundException(nameof(User), creatorUserId);
        }

        var project = Project.Create(name, description, user.Id);

        await projectRepository.AddAsync(project);
        await unitOfWork.CompleteAsync();

        return project;
    }

    public async Task<Project> UpdateProjectAsync(long id, string? name, string? description, ProjectStatus status,
        ProjectVisibility visibility, bool archived, long creatorUserId)
    {
        var project = await projectRepository.GetByIdAsync(id);
        if (project is null)
            throw new NotFoundException(nameof(Project), id);

        var projectMember = project.ProjectMembers.SingleOrDefault(x =>
            x.UserId == creatorUserId && x.Role is ProjectRole.Admin or ProjectRole.Owner);

        if (projectMember is null) throw new AccessDeniedException("You don't have access to this project");

        project.Update(name, description, status, visibility, archived, creatorUserId);
        
        await projectRepository.UpdateAsync(project);
        await unitOfWork.CompleteAsync();
        
        return project;
    }
    
    public async Task<Project> GetProjectByIdAsync(long id, long currentUserId)
    {
        var project = await projectRepository.GetByIdAsync(id);
        if (project is null)
            throw new NotFoundException(nameof(Project), id);
        
        if (project.ProjectMembers.All(x => x.UserId != currentUserId))
            throw new AccessDeniedException("You don't have access to this project");
        
        return project;
    }

    public async Task<long> ArchiveProjectAsync(long id, long currentUserId)
    {
        var project = await projectRepository.GetByIdAsync(id);
        if (project is null)
            throw new NotFoundException(nameof(Project), id);
        
        project.Archive(currentUserId);
        
        await projectRepository.UpdateAsync(project);

        return project.Id;
    }
    
    //TODO: Dodaj za Unarchive funkcionalnost

    public async Task<long> AddProjectMember(long projectId, long newMemberId, ProjectRole role, long inviterUserId)
    {
        var project = await projectRepository.GetByIdAsync(projectId);
        if (project is null)
            throw new NotFoundException(nameof(Project), projectId);
        
        
        project.AddMember(newMemberId, role, inviterUserId);
        
        await projectRepository.UpdateAsync(project);
        
        return project.Id;
    }

    public async Task RemoveProjectMember(long projectId,long removedUserId, long removerUserId)
    {
        var project = await projectRepository.GetByIdAsync(projectId);
        if (project is null)
            throw new NotFoundException(nameof(Project), projectId);
        
        project.RemoveMember(removedUserId, removerUserId);
        
        await projectRepository.UpdateAsync(project);
    }
}