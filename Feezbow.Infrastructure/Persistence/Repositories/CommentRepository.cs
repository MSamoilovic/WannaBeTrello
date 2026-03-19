using Microsoft.EntityFrameworkCore;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Interfaces.Repositories;
using Feezbow.Domain.Specifications.CommentSpecifications;

namespace Feezbow.Infrastructure.Persistence.Repositories;

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
