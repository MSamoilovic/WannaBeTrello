using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces;
using WannabeTrello.Domain.Interfaces.Repositories;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Domain.Services;

public class CommentService(ICommentRepository commentRepository, IUnitOfWork unitOfWork): ICommentService
{
    public async Task UpdateCommentAsync(long commentId, string? newContent, long userId, CancellationToken ct)
    {
        var comment = await commentRepository.GetByIdAsync(commentId, ct);
        if (comment == null)
            throw new NotFoundException(nameof(Comment), commentId);
        
        if(comment.UserId != userId)
            throw new AccessDeniedException("Samo autor komentara može menjati komentar.");
        
        comment.UpdateContent(newContent, userId);
        await unitOfWork.CompleteAsync(ct);
    }

    public async Task<Comment> GetCommentByIdAsync(long commentId, CancellationToken ct)
    {
        var comment = await commentRepository.GetByIdAsync(commentId, ct);
        return comment ?? throw new NotFoundException(nameof(Comment), commentId);
    }

    public async Task<IReadOnlyList<Comment>> GetCommentsByTaskId(long taskId, CancellationToken ct)
    {
        var comments = await commentRepository.GetCommentsByTaskIdAsync(taskId, ct);
        return comments;
    }

    public async Task DeleteCommentAsync(long commentId, long userId, CancellationToken ct)
    {
        var comment = await commentRepository.GetByIdAsync(commentId, ct);
        if (comment == null)
            throw new NotFoundException(nameof(Comment), commentId);
        
        if(comment.UserId != userId)
            throw new AccessDeniedException("Samo autor komentara može obrisati komentar.");
        
        comment.Delete(userId);
        await unitOfWork.CompleteAsync(ct);
    }

    public async Task RestoreCommentAsync(long commentId, long userId, CancellationToken ct)
    {
        var comment = await commentRepository.GetByIdAsync(commentId, ct);
        if (comment == null)
            throw new NotFoundException(nameof(Comment), commentId);
        
        if(comment.UserId != userId)
            throw new AccessDeniedException("Samo autor komentara može vratiti komentar.");
        
        if (!comment.IsDeleted)
            throw new BusinessRuleValidationException("Komentar nije obrisan.");
        
        comment.Restore(userId);
        await unitOfWork.CompleteAsync(ct);
    }
}