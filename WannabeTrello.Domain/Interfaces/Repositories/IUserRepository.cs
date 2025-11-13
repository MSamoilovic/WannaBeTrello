using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Interfaces.Repositories;

public interface IUserRepository: IRepository<User>
{
    Task<User?> GetUserProfileDetailsAsync(long userId, CancellationToken cancellationToken);
    Task<User?> GetUserProfileAsync(long userId, CancellationToken cancellationToken);
    IQueryable<User> SearchUsers();
    Task<IReadOnlyList<Project>> GetUserOwnedProjectsAsync(long userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Board>> GetUserBoardsAsync(long userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<BoardTask>> GetUserAssignedTasksAsync(long userId, CancellationToken cancellationToken);
}