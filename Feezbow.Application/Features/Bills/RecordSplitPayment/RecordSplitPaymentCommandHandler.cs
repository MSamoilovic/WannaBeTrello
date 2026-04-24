using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Bills.RecordSplitPayment;

public class RecordSplitPaymentCommandHandler(
    IBillService billService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<RecordSplitPaymentCommand, RecordSplitPaymentCommandResponse>
{
    public async Task<RecordSplitPaymentCommandResponse> Handle(
        RecordSplitPaymentCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var projectId = await billService.RecordSplitPaymentAsync(
            request.BillId,
            request.UserId,
            userId,
            cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.ProjectBills(projectId), cancellationToken);
        await cacheService.RemoveByPrefixAsync(CacheKeys.ProjectBudgetSummaryPrefix(projectId), cancellationToken);

        return new RecordSplitPaymentCommandResponse(Result<bool>.Success(true, "Payment recorded."));
    }
}
