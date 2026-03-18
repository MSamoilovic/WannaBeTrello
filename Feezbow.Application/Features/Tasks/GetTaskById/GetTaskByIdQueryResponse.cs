using WannabeTrello.Application.Features.Boards.GetBoardById;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Application.Features.Tasks.GetTaskById;

public class GetTaskByIdQueryResponse
{
    
        public long Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public long ColumnId { get; set; }
        public long? AssigneeId { get; set; }
        public ICollection<GetTaskByIdCommentResponse> Comments { get; set; } = new List<GetTaskByIdCommentResponse>();
        
        public static GetTaskByIdQueryResponse FromEntity(BoardTask task)
        {
            return new GetTaskByIdQueryResponse
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Priority = task.Priority,
                DueDate = task.DueDate,
                ColumnId = task.ColumnId,
                AssigneeId = task.AssigneeId,
                Comments = task.Comments?.Select(GetTaskByIdCommentResponse.FromEntity).ToList() ?? []
            };
        }
}

public class GetTaskByIdCommentResponse
{
    public long Id { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public long UserId { get; set; }
    
    public static GetTaskByIdCommentResponse FromEntity(Comment comment)
    {
        return new GetTaskByIdCommentResponse
        {
            Id = comment.Id,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UserId = comment.UserId,
            
        };
    }
}