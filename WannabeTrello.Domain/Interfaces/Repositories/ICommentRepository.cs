using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Interfaces.Repositories;

public interface ICommentRepository
{
    Task<Comment?> GetByIdAsync(long id);
    Task<IEnumerable<Comment>> GetCommentsByTaskIdAsync(long taskId);
    Task AddAsync(Comment comment);
    Task UpdateAsync(Comment comment);
    Task DeleteAsync(long id);
}