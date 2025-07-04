using Microsoft.EntityFrameworkCore;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Infrastructure.Persistence.Repositories;

public class CommentRepository: Repository<Comment>, ICommentRepository
{
    public CommentRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    
    public async Task<IEnumerable<Comment>> GetCommentsByTaskIdAsync(long taskId)
    {
        return await _dbSet
            .Where(c => c.TaskId == taskId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }
   
    public async Task AddAsync(Comment comment) => await base.AddAsync(comment);
    public async Task<Comment?> GetByIdAsync(long id) => await base.GetByIdAsync(id);
    
    public async Task UpdateAsync(Comment comment) => base.Update(comment);
    
    public async Task DeleteAsync(long id)
    {
        var comment = await GetByIdAsync(id);
        if (comment != null) base.Delete(comment);
    }
}
