using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;

namespace Feezbow.Application.Features.Bills.DeleteBill;

public class DeleteBillCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<DeleteBillCommand, DeleteBillCommandResponse>
{
    public async Task<DeleteBillCommandResponse> Handle(
        DeleteBillCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var bill = await unitOfWork.Bills.GetByIdAsync(request.BillId, cancellationToken)
            ?? throw new NotFoundException("Bill", request.BillId);

        if (!bill.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        var projectId = bill.ProjectId;

        unitOfWork.Bills.Remove(bill);
        await unitOfWork.CompleteAsync(cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.ProjectBills(projectId), cancellationToken);

        return new DeleteBillCommandResponse(Result<bool>.Success(true, "Bill deleted successfully."));
    }
}
