using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Bills.CancelBillRecurrence;

public class CancelBillRecurrenceCommandHandler(
    IBillService billService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<CancelBillRecurrenceCommand, CancelBillRecurrenceCommandResponse>
{
    public async Task<CancelBillRecurrenceCommandResponse> Handle(
        CancelBillRecurrenceCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var projectId = await billService.CancelBillRecurrenceAsync(
            request.BillId, userId, cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.ProjectBills(projectId), cancellationToken);

        return new CancelBillRecurrenceCommandResponse(
            Result<long>.Success(request.BillId, "Bill recurrence cancelled."));
    }
}
