using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Bills.CreateBill;

public class CreateBillCommandHandler(
    IBillService billService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<CreateBillCommand, CreateBillCommandResponse>
{
    public async Task<CreateBillCommandResponse> Handle(
        CreateBillCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var bill = await billService.CreateBillAsync(
            request.ProjectId,
            userId,
            request.Title,
            request.Amount,
            request.DueDate,
            request.Currency,
            request.Description,
            request.Category,
            request.SplitUserIds,
            request.RecurrenceFrequency,
            request.RecurrenceInterval,
            request.RecurrenceDaysOfWeek,
            request.RecurrenceEndDate,
            cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.ProjectBills(request.ProjectId), cancellationToken);

        return new CreateBillCommandResponse(Result<long>.Success(bill.Id, "Bill created successfully."));
    }
}
