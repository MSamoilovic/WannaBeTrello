using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Application.Features.Boards.UpdateBoard;

public class UpdateBoardCommandResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public long ProjectId { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? CreatedBy { get; set; }

    public ICollection<UpdateBoardColumnResponse> Columns { get; set; } = new List<UpdateBoardColumnResponse>();
    public ICollection<UpdateBoardMemberResponse> BoardMembers { get; set; } = new List<UpdateBoardMemberResponse>();
    
    public static UpdateBoardCommandResponse FromEntity(Board board)
    {
        return new UpdateBoardCommandResponse
        {
            Id = board.Id,
            Name = board.Name,
            Description = board.Description,
            ProjectId = board.ProjectId,
            CreatedAt = board.CreatedAt,
            CreatedBy = board.CreatedBy,
            Columns = board.Columns?.Select(UpdateBoardColumnResponse.FromEntity).ToList() ?? [],
            BoardMembers = board.BoardMembers?.Select(UpdateBoardMemberResponse.FromEntity).ToList() ?? []
        };
    }
}

public class UpdateBoardColumnResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public int Order { get; set; }
    
    //Kolekcija TaskDTO

    public static UpdateBoardColumnResponse FromEntity(Column column)
    {
        return new UpdateBoardColumnResponse
        {
            Id = column.Id,
            Name = column.Name,
            Order = column.Order,
            //Implementiraj Tasks
        };
    }
}

public class UpdateBoardMemberResponse
{
    public long UserId { get; set; }
    public BoardRole Role { get; set; }

    public static UpdateBoardMemberResponse FromEntity(BoardMember boardMember)
    {
        return new UpdateBoardMemberResponse
        {
            UserId = boardMember.UserId,
            Role = boardMember.Role
            //Dodaj ceo User objekat
        };
    }
}