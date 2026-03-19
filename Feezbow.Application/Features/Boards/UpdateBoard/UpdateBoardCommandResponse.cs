using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;

namespace Feezbow.Application.Features.Boards.UpdateBoard;

public class UpdateBoardCommandResponse
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public long ProjectId { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    
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
        };
    }
}