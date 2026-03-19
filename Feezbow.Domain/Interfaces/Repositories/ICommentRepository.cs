using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Interfaces.Repositories;

public interface ICommentRepository: IRepository<Comment>
{
    Task<Comment?> GetCommentDetailsByIdAsync(long commentId, CancellationToken ct);
    Task<IReadOnlyList<Comment>> GetCommentsByTaskIdAsync(long taskId, CancellationToken ct);
    
}