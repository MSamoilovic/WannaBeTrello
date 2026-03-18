using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Application.Features.Tasks.SearchTasks
{
    public class SearchTaskQueryResponse
    {

        public long Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public long ColumnId { get; set; }
        public long? AssigneeId { get; set; }
        public ICollection<SearchTaskQueryResponseCommentResponse> Comments { get; set; } = [];

        public static SearchTaskQueryResponse FromEntity(BoardTask task)
        {
            return new SearchTaskQueryResponse
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Priority = task.Priority,
                DueDate = task.DueDate,
                ColumnId = task.ColumnId,
                AssigneeId = task.AssigneeId,
                Comments = task.Comments?.Select(SearchTaskQueryResponseCommentResponse.FromEntity).ToList() ?? []
            };
        }
    }

    public class SearchTaskQueryResponseCommentResponse
    {
        public long Id { get; set; }
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public long UserId { get; set; }

        public static SearchTaskQueryResponseCommentResponse FromEntity(Comment comment)
        {
            return new SearchTaskQueryResponseCommentResponse
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UserId = comment.UserId,

            };
        }
    }
}
