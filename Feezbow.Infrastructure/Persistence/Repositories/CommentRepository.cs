using Microsoft.EntityFrameworkCore;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces.Repositories;
using WannabeTrello.Domain.Specifications.CommentSpecifications;

namespace WannabeTrello.Infrastructure.Persistence.Repositories;

public class CommentRepository(ApplicationDbContext dbContext) : Repository<Comment>(dbContext), ICommentRepository
{
    public Task<Comment?> GetCommentDetailsByIdAsync(long commentId, CancellationToken ct)
    {
        var spec = new GetCommentDetailsSpecification(commentId);
        return GetSingleAsync(spec, ct);
    }

    public Task<IReadOnlyList<Comment>> GetCommentsByTaskIdAsync(long taskId, CancellationToken ct)
    {
        var spec = new GetCommentsByTaskIdSpecification(taskId);
        return GetAsync(spec, ct);
    }
}
