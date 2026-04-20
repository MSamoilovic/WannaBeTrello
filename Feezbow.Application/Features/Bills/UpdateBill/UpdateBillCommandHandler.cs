using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;

namespace Feezbow.Application.Features.Bills.UpdateBill;

public class UpdateBillCommandHandler(
    IUnitOfWork unitOfWork,
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

        var bill = await unitOfWork.Bills.GetByIdAsync(request.BillId, cancellationToken)
            ?? throw new NotFoundException("Bill", request.BillId);

        if (!bill.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        bill.Update(request.Title, request.Description, request.Category, request.Amount, request.DueDate, userId);

        await unitOfWork.CompleteAsync(cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.ProjectBills(bill.ProjectId), cancellationToken);

        return new UpdateBillCommandResponse(Result<bool>.Success(true, "Bill updated successfully."));
    }
}
