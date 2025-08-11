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
    
    public async Task<Project> GetProjectByIdAsync(long id)
    {
        var project = await projectRepository.GetByIdAsync(id);
        if (project is null)
            throw new NotFoundException(nameof(Project), id);
        
        //TODO: Dodati dodatne validacije

        return project;
    }
}