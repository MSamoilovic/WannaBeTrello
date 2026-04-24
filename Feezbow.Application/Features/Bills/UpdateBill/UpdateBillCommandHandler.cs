using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Bills.UpdateBill;

public class UpdateBillCommandHandler(
    IBillService billService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<UpdateBillCommand, UpdateBillCommandResponse>
{
    public async Task<UpdateBillCommandResponse> Handle(
        UpdateBillCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var projectId = await billService.UpdateBillAsync(
            request.BillId,
            userId,
            request.Title,
            request.Description,
            request.Category,
            request.Amount,
            request.DueDate,
            cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.ProjectBills(projectId), cancellationToken);
        await cacheService.RemoveByPrefixAsync(CacheKeys.ProjectBudgetSummaryPrefix(projectId), cancellationToken);

        return new UpdateBillCommandResponse(Result<bool>.Success(true, "Bill updated successfully."));
    }
}
