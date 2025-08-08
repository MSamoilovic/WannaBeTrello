using WannabeTrello.Domain.Entities;
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
    
}