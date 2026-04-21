using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Bills.MarkBillPaid;

public class MarkBillPaidCommandHandler(
    IBillService billService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<MarkBillPaidCommand, MarkBillPaidCommandResponse>
{
    public async Task<MarkBillPaidCommandResponse> Handle(
        MarkBillPaidCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var (projectId, nextBillId) = await billService.MarkBillPaidAsync(request.BillId, userId, cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.ProjectBills(projectId), cancellationToken);

        var message = nextBillId.HasValue
            ? $"Bill paid. Next occurrence created (ID: {nextBillId.Value})."
            : "Bill paid.";

        return new MarkBillPaidCommandResponse(Result<long?>.Success(nextBillId, message));
    }
}
