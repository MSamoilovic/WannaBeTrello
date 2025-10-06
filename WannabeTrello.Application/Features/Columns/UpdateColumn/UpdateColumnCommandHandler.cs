using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Columns.UpdateColumn;

public class UpdateColumnCommandHandler(IColumnService columnService, ICurrentUserService currentUserService)
    : IRequestHandler<UpdateColumnCommand, UpdateColumnCommandResponse>
{
    public async Task<UpdateColumnCommandResponse> Handle(UpdateColumnCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            //return Result<UpdateColumnCommandResponse>.Failure("User is not authenticated.");
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        var updatedColumn = await columnService.UpdateColumnAsync(
            request.ColumnId,
            request.NewName,
            request.WipLimit,
            currentUserService.UserId.Value,
            cancellationToken
        );

       return UpdateColumnCommandResponse.FromEntity(updatedColumn);
    }
}