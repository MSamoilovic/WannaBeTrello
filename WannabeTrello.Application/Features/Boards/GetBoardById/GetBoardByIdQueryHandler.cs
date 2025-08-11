using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Application.Features.Boards.GetBoardById;

public class GetBoardByIdQueryHandler(IBoardRepository boardRepository, ICurrentUserService currentUserService)
    : IRequestHandler<GetBoardByIdQuery, GetBoardByIdQueryResponse>
{
    public async Task<GetBoardByIdQueryResponse> Handle(GetBoardByIdQuery request, CancellationToken cancellationToken)
    {
        //TODO: Odradi refactor da se zove Board service
        var board = await boardRepository.GetBoardWithDetailsAsync(request.Id);

        if (board == null)
        {
            throw new NotFoundException(nameof(board), request.Id);
        }

        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue ||
            board.BoardMembers.All(bm => bm.UserId != currentUserService.UserId.Value))
        {
            throw new AccessDeniedException("Nemate pristup za pregled ovog boarda.");
        }

        return GetBoardByIdQueryResponse.FromEntity(board);
    }
}