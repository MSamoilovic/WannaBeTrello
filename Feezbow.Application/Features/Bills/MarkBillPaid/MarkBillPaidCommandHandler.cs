using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;

namespace Feezbow.Application.Features.Bills.MarkBillPaid;

public class MarkBillPaidCommandHandler(
    IUnitOfWork unitOfWork,
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

        var bill = await unitOfWork.Bills.GetByIdAsync(request.BillId, cancellationToken)
            ?? throw new NotFoundException("Bill", request.BillId);

        if (!bill.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        var nextBill = bill.MarkFullyPaid(userId);

        long? nextBillId = null;
        if (nextBill is not null)
        {
            await unitOfWork.Bills.AddAsync(nextBill, cancellationToken);
        }

        await unitOfWork.CompleteAsync(cancellationToken);

        if (nextBill is not null)
            nextBillId = nextBill.Id;

        await cacheService.RemoveAsync(CacheKeys.ProjectBills(bill.ProjectId), cancellationToken);

        var message = nextBillId.HasValue
            ? $"Bill paid. Next occurrence created (ID: {nextBillId.Value})."
            : "Bill paid.";

        return new MarkBillPaidCommandResponse(Result<long?>.Success(nextBillId, message));
    }
}
