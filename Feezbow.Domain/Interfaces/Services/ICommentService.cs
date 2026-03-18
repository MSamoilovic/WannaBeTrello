using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Interfaces.Services;

public interface ICommentService
{
    Task UpdateCommentAsync(long commentId, string? newContent, long userId, CancellationToken ct);
    Task<Comment> GetCommentByIdAsync(long commentId, CancellationToken ct);
    Task<IReadOnlyList<Comment>> GetCommentsByTaskId(long taskId, CancellationToken ct);
    Task DeleteCommentAsync(long commentId, long userId, CancellationToken ct);
    Task RestoreCommentAsync(long commentId, long userId, CancellationToken ct);
}