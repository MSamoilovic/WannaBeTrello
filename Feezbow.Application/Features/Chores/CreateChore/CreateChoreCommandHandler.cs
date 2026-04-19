using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.ValueObjects;

namespace Feezbow.Application.Features.Chores.CreateChore;

public class CreateChoreCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<CreateChoreCommand, CreateChoreCommandResponse>
{
    public async Task<CreateChoreCommandResponse> Handle(
        CreateChoreCommand request, CancellationToken cancellationToken)
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

        var chore = HouseholdChore.Create(
            request.ProjectId,
            request.Title,
            userId,
            request.Description,
            request.AssignedToUserId,
            request.DueDate,
            recurrence,
            request.Priority);

        await unitOfWork.Chores.AddAsync(chore, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.ProjectChores(request.ProjectId), cancellationToken);

        return new CreateChoreCommandResponse(Result<long>.Success(chore.Id, "Chore created successfully."));
    }
}
