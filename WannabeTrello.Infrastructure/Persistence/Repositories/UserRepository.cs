using Microsoft.EntityFrameworkCore;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Infrastructure.Persistence.Repositories;

public class UserRepository: Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.UserName == username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }
    

    public async Task AddAsync(User user) => await base.AddAsync(user);
    public async Task<IEnumerable<User>> GetAllAsync() => await base.GetAllAsync();
    public async Task<User?> GetByIdAsync(long id) => await base.GetByIdAsync(id);
    public async Task UpdateAsync(User user) => base.Update(user);
    public async Task DeleteAsync(long id)
    {
        var user = await GetByIdAsync(id);
        if (user != null) base.Delete(user);
    }
}