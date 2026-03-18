using MediatR;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Application.Features.Labels.GetLabelsByBoard;

public class GetLabelsByBoardQueryHandler(
    ILabelRepository labelRepository,
    ICurrentUserService currentUserService,
    ICacheService cacheService) : IRequestHandler<GetLabelsByBoardQuery, IReadOnlyList<GetLabelsByBoardQueryResponse>>
{
    public async Task<IReadOnlyList<GetLabelsByBoardQueryResponse>> Handle(GetLabelsByBoardQuery request, CancellationToken cancellationToken)
    {
        if (currentUserService.UserId is null)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var labels = await cacheService.GetOrSetAsync(
            CacheKeys.BoardLabels(request.BoardId),
            () => labelRepository.GetByBoardIdAsync(request.BoardId, cancellationToken),
            CacheExpiration.Short,
            cancellationToken);

        return labels.Select(GetLabelsByBoardQueryResponse.FromEntity).ToList();
    }
}
