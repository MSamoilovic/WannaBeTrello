using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Bills.GetBillsByProject;

public class GetBillsByProjectQueryHandler(
    IBillService billService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<GetBillsByProjectQuery, IReadOnlyList<BillDto>>
{
    public async Task<IReadOnlyList<BillDto>> Handle(
        GetBillsByProjectQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        if (request.IncludePaid)
        {
            var all = await billService.GetByProjectAsync(request.ProjectId, userId, true, cancellationToken);
            return all.Select(BillDto.FromEntity).ToList();
        }

        var cached = await cacheService.GetOrSetAsync(
            CacheKeys.ProjectBills(request.ProjectId),
            async () =>
            {
                var bills = await billService.GetByProjectAsync(request.ProjectId, userId, false, cancellationToken);
                return bills.Select(BillDto.FromEntity).ToList();
            },
            CacheExpiration.Medium,
            cancellationToken);

        return cached ?? new List<BillDto>();
    }
}
