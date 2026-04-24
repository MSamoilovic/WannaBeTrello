using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Bills.UpdateBillRecurrence;

public class UpdateBillRecurrenceCommandHandler(
    IBillService billService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<UpdateBillRecurrenceCommand, UpdateBillRecurrenceCommandResponse>
{
    public async Task<UpdateBillRecurrenceCommandResponse> Handle(
        UpdateBillRecurrenceCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var projectId = await billService.UpdateBillRecurrenceAsync(
            request.BillId,
            userId,
            request.Frequency,
            request.Interval,
            request.DaysOfWeek,
            request.EndDate,
            cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.ProjectBills(projectId), cancellationToken);

        return new UpdateBillRecurrenceCommandResponse(
            Result<long>.Success(request.BillId, "Bill recurrence updated."));
    }
}
