using MediatR;

namespace WannabeTrello.Application.Features.Boards.GetBoardById;

public class GetBoardByIdQuery: IRequest<GetBoardByIdQueryResponse>
{
    public long Id { get; set; }
}