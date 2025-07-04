using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Application.Features.Tasks.GetTasksByBoardId;

public class GetTaskByBoardIdQueryResponse
{
    
    public long Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public long ColumnId { get; set; }
    public long? AssigneeId { get; set; }
    public ICollection<GetTaskByBoardIdCommentResponse> Comments { get; set; } = new List<GetTaskByBoardIdCommentResponse>();
        
    public static GetTaskByBoardIdQueryResponse FromEntity(BoardTask task)
    {
        return new GetTaskByBoardIdQueryResponse
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Priority = task.Priority,
            DueDate = task.DueDate,
            ColumnId = task.ColumnId,
            AssigneeId = task.AssigneeId,
            Comments = task.Comments?.Select(GetTaskByBoardIdCommentResponse.FromEntity).ToList() ?? []
        };
    }
}

public class GetTaskByBoardIdCommentResponse
{
    public long Id { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public long UserId { get; set; }
    
    public static GetTaskByBoardIdCommentResponse FromEntity(Comment comment)
    {
        return new GetTaskByBoardIdCommentResponse
        {
            Id = comment.Id,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UserId = comment.UserId,
            
        };
    }
}