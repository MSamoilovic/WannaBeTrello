using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Application.Features.Boards.GetBoardById;

public class GetBoardByIdQueryResponse
{
        public long Id { get; set; }
        public string? Name { get; set; } = null!;
        public string? Description { get; set; }
        public long ProjectId { get; set; }
        public DateTime CreatedAt { get; set; }
        public long? CreatedBy { get; set; }
        
        public ICollection<ColumnResponse> Columns { get; set; } = [];
        // public ICollection<BoardMemberResponse> BoardMembers { get; set; } = new List<BoardMemberResponse>();
        
        public static GetBoardByIdQueryResponse FromEntity(Board board)
        {
            return new GetBoardByIdQueryResponse
            {
                Id = board.Id,
                Name = board.Name,
                Description = board.Description,
                ProjectId = board.ProjectId,
                CreatedAt = board.CreatedAt,
                CreatedBy = board.CreatedBy,
                //TODO: RESI OVO KASNIJE
                Columns = board.Columns?.Select(ColumnResponse.FromEntity).ToList() ?? [],
                // BoardMembers = board.BoardMembers?.Select(BoardMemberResponse.FromEntity).ToList() ?? new List<BoardMemberResponse>()
            };
        }
    }

    public class ColumnResponse
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public int Order { get; set; }
        // public ICollection<SearchTaskResponse> Tasks { get; set; } = new List<SearchTaskResponse>();

        public static ColumnResponse FromEntity(Column column)
        {
            return new ColumnResponse
            {
                Id = column.Id,
                Name = column.Name,
                Order = column.Order,
                // Tasks = column.Tasks?.Select(SearchTaskResponse.FromEntity).ToList() ?? new List<SearchTaskResponse>()
            };
        }
    }

    
    public class SearchTaskResponse
    {
        public long Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public long ColumnId { get; set; }
        public long? AssigneeId { get; set; }
        public UserResponse? Assignee { get; set; }
        public ICollection<CommentResponse> Comments { get; set; } = new List<CommentResponse>();
        
        public static SearchTaskResponse FromEntity(BoardTask task)
        {
            return new SearchTaskResponse
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Priority = task.Priority,
                DueDate = task.DueDate,
                ColumnId = task.ColumnId,
                AssigneeId = task.AssigneeId,
                Assignee = task.Assignee != null ? UserResponse.FromEntity(task.Assignee) : null,
                Comments = task.Comments?.Select(CommentResponse.FromEntity).ToList() ?? [],
            };
        }
    }

    public class UserResponse
    {
        public long Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;

        public static UserResponse FromEntity(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                Username = user?.UserName!,
                Email = user?.Email!
            };
        }
    }

    public class CommentResponse
    {
        public long Id { get; set; }
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public long UserId { get; set; }
        public UserResponse User { get; set; } = null!;

        public static CommentResponse FromEntity(Comment comment)
        {
            return new CommentResponse
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UserId = comment.UserId,
                User = UserResponse.FromEntity(comment.User)
            };
        }
    }

    public class BoardMemberResponse
    {
        public long UserId { get; set; }
        public UserResponse User { get; set; } = null!;
        public BoardRole Role { get; set; }

        public static BoardMemberResponse FromEntity(BoardMember boardMember)
        {
            return new BoardMemberResponse
            {
                UserId = boardMember.UserId,
                User = UserResponse.FromEntity(boardMember?.User!),
                Role = boardMember!.Role
            };
        }
    }