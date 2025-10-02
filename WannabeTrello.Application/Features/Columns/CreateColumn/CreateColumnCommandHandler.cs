using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities.Common;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Columns.CreateColumn;

public class CreateColumnCommandHandler(IColumnService boardService, ICurrentUserService currentUserService)
    : IRequestHandler<CreateColumnCommand, CreateColumnCommandResponse>
{
    public async Task<CreateColumnCommandResponse> Handle(CreateColumnCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }
        
        
        
        var column = await boardService.CreateColumnAsync(
            request.BoardId,
            request.Name!,
            request.Order,
            currentUserService!.UserId ?? 0,
            cancellationToken
        );
        
        var result =  Result<long>.Success(column.Id, "Column created successfully");
        return new CreateColumnCommandResponse(result);
    }
}