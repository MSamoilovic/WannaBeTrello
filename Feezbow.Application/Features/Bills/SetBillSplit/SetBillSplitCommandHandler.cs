using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Bills.SetBillSplit;

public class SetBillSplitCommandHandler(
    IBillService billService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<SetBillSplitCommand, SetBillSplitCommandResponse>
{
    public async Task<SetBillSplitCommandResponse> Handle(
        SetBillSplitCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var customShares = request.CustomShares?.Select(s => (s.UserId, s.Amount)).ToList();

        var projectId = await billService.SetBillSplitAsync(
            request.BillId,
            userId,
            request.EqualSplitUserIds,
            customShares,
            cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.ProjectBills(projectId), cancellationToken);

        return new SetBillSplitCommandResponse(Result<bool>.Success(true, "Bill split updated."));
    }
}
