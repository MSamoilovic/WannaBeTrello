using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Interfaces.Repositories;

public interface IUserRepository: IRepository<User>
{
    Task<User?> GetByIdAsync(long id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(long id);
}