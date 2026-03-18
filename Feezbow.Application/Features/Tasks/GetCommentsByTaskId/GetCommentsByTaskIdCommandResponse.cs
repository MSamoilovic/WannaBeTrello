using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Application.Features.Tasks.GetCommentsByTaskId;

public class GetCommentsByTaskIdCommandResponse
{
    public long Id { get; private set; }
    public string? Content { get; private set; } 
    public long TaskId { get; private set; }
    public long UserId { get; private set; } 
    public bool IsDeleted { get; private set; }
    public bool IsEdited { get; private set; }
    public DateTime? EditedAt { get; private set; }

    public static GetCommentsByTaskIdCommandResponse FromEntity(Comment comment)
    {
        return new GetCommentsByTaskIdCommandResponse()
        {
            Id = comment.Id,
            Content = comment.Content,
            TaskId = comment.TaskId,
            UserId = comment.UserId,
            IsDeleted = comment.IsDeleted,
            IsEdited = comment.IsEdited,
            EditedAt = comment.EditedAt
        };
    }
}