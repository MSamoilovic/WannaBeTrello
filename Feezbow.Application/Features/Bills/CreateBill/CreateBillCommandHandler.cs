using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.ValueObjects;

namespace Feezbow.Application.Features.Bills.CreateBill;

public class CreateBillCommandHandler(
    IUnitOfWork unitOfWork,
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

        var project = await unitOfWork.Projects.GetProjectWithMembersAsync(request.ProjectId, cancellationToken)
            ?? throw new NotFoundException("Project", request.ProjectId);

        if (!project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        RecurrenceRule? recurrence = null;
        if (request.RecurrenceFrequency.HasValue)
        {
            recurrence = RecurrenceRule.Create(
                request.RecurrenceFrequency.Value,
                request.RecurrenceInterval,
                request.RecurrenceDaysOfWeek,
                request.RecurrenceEndDate);
        }

        var bill = Bill.Create(
            request.ProjectId,
            request.Title,
            request.Amount,
            request.DueDate,
            userId,
            request.Currency,
            request.Description,
            request.Category,
            recurrence);

        if (request.SplitUserIds is { Count: > 0 })
        {
            foreach (var splitUserId in request.SplitUserIds)
            {
                if (!project.IsMember(splitUserId))
                    throw new BusinessRuleValidationException($"User {splitUserId} is not a member of this project.");
            }

            bill.SetEqualSplit(request.SplitUserIds, userId);
        }

        await unitOfWork.Bills.AddAsync(bill, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.ProjectBills(request.ProjectId), cancellationToken);

        return new CreateBillCommandResponse(Result<long>.Success(bill.Id, "Bill created successfully."));
    }
}
