using MediatR;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Repositories;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Columns.GetColumn;

public class GetColumnByIdQueryHandler(
    IColumnService columnService, 
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<GetColumnByIdQuery, GetColumnByIdQueryResponse>
{
    public async Task<GetColumnByIdQueryResponse> Handle(GetColumnByIdQuery request, CancellationToken cancellationToken)
    {
       if (currentUserService.UserId is null)
            throw new UnauthorizedAccessException("User is not authenticated");

       var cacheKey = CacheKeys.Column(request.ColumnId);
       var userId = currentUserService.UserId.Value;

       var column = await cacheService.GetOrSetAsync(
           cacheKey,
           () => columnService.GetColumnByIdAsync(request.ColumnId, userId, cancellationToken),
           CacheExpiration.Short,
           cancellationToken
       );

       return GetColumnByIdQueryResponse.FromEntity(column);
    }
}