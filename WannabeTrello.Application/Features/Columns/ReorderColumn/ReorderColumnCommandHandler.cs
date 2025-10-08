using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Columns.ReorderColumn;

public class ReorderColumnCommandHandler(IBoardService boardService, ICurrentUserService currentUserService)
    : IRequestHandler<ReorderColumnCommand, ReorderColumnCommandResponse>
{
    public async Task<ReorderColumnCommandResponse> Handle(ReorderColumnCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException();
        }

        await boardService.ReorderColumnsAsync(
            request.BoardId,
            request.ColumnOrders,
            currentUserService.UserId.Value,
            cancellationToken);

        var result = Domain.Entities.Common.Result<long>.Success(request.BoardId, "Columns reordered successfully");
        return new ReorderColumnCommandResponse(result);
    }
}