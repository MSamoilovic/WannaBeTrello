using MediatR;

namespace WannabeTrello.Application.Features.Boards.UpdateBoard;

public class UpdateBoardCommand: IRequest<UpdateBoardCommandResponse>
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }    
}