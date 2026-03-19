using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Columns.GetColumn;

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