using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Repositories;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Columns.GetColumn;

public class GetColumnByIdQueryHandler(IColumnService columnService, ICurrentUserService currentUserService): IRequestHandler<GetColumnByIdQuery, GetColumnByIdQueryResponse>
{
    public async Task<GetColumnByIdQueryResponse> Handle(GetColumnByIdQuery request, CancellationToken cancellationToken)
    {
       if (currentUserService.UserId is null)
            throw new UnauthorizedAccessException("User is not authenticated");

       var column = await columnService.GetColumnByIdAsync(request.ColumnId, currentUserService.UserId.Value, cancellationToken);
       return GetColumnByIdQueryResponse.FromEntity(column);
    }
}