using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Columns.DeleteColumn;

public class DeleteColumnCommandHandler(IColumnService columnService, ICurrentUserService currentUserService)
    : IRequestHandler<DeleteColumnCommand, DeleteColumnCommandResponse>
{
    public async Task<DeleteColumnCommandResponse> Handle(DeleteColumnCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated");
        
        var userId = currentUserService.UserId;
        
        var columnId = await columnService.DeleteColumnAsync(request.ColumnId, userId, cancellationToken);
        
        var result = Result<long>.Success(columnId, "Column deleted successfully");
        return new DeleteColumnCommandResponse(result);
    }
}