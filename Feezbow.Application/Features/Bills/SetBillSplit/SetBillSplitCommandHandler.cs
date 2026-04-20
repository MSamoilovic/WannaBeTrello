using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;

namespace Feezbow.Application.Features.Bills.SetBillSplit;

public class SetBillSplitCommandHandler(
    IUnitOfWork unitOfWork,
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

        var bill = await unitOfWork.Bills.GetByIdAsync(request.BillId, cancellationToken)
            ?? throw new NotFoundException("Bill", request.BillId);

        if (!bill.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        if (request.CustomShares is { Count: > 0 })
        {
            foreach (var share in request.CustomShares)
            {
                if (!bill.Project.IsMember(share.UserId))
                    throw new BusinessRuleValidationException($"User {share.UserId} is not a member of this project.");
            }

            bill.SetCustomSplit(request.CustomShares.Select(s => (s.UserId, s.Amount)).ToList(), userId);
        }
        else if (request.EqualSplitUserIds is { Count: > 0 })
        {
            foreach (var splitUserId in request.EqualSplitUserIds)
            {
                if (!bill.Project.IsMember(splitUserId))
                    throw new BusinessRuleValidationException($"User {splitUserId} is not a member of this project.");
            }

            bill.SetEqualSplit(request.EqualSplitUserIds, userId);
        }
        else
        {
            throw new BusinessRuleValidationException("Either EqualSplitUserIds or CustomShares must be provided.");
        }

        await unitOfWork.CompleteAsync(cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.ProjectBills(bill.ProjectId), cancellationToken);

        return new SetBillSplitCommandResponse(Result<bool>.Success(true, "Bill split updated."));
    }
}
