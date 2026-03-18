using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Application.Features.Users.GetUserBoards;

public class GetUserBoardsQueryResponse
{
    public IReadOnlyList<UserBoardDto> Boards { get; init; } = new List<UserBoardDto>();

    public static GetUserBoardsQueryResponse FromEntities(IReadOnlyList<Board> boards)
    {
        return new GetUserBoardsQueryResponse
        {
            Boards = boards.Select(UserBoardDto.FromEntity).ToList()
        };
    }
}

public class UserBoardDto
{
    public long Id { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public long ProjectId { get; init; }
    public string? ProjectName { get; init; }
    public bool IsArchived { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastModifiedAt { get; init; }
    public int MembersCount { get; init; }
    public int ColumnsCount { get; init; }
    public BoardRole? UserRole { get; init; }

    public static UserBoardDto FromEntity(Board board)
    {
        return new UserBoardDto
        {
            Id = board.Id,
            Name = board.Name,
            Description = board.Description,
            ProjectId = board.ProjectId,
            ProjectName = board.Project?.Name,
            IsArchived = board.IsArchived,
            CreatedAt = board.CreatedAt,
            LastModifiedAt = board.LastModifiedAt,
            MembersCount = board.BoardMembers?.Count ?? 0,
            ColumnsCount = board.Columns?.Count ?? 0,
            UserRole = board.BoardMembers?.FirstOrDefault()?.Role
        };
    }
}

