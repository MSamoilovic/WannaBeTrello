using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;

namespace Feezbow.Application.Features.Tasks.GetRecurringTasks;

public class GetRecurringTasksQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetRecurringTasksQuery, IReadOnlyList<GetRecurringTasksQueryResponse>>
{
    public async Task<IReadOnlyList<GetRecurringTasksQueryResponse>> Handle(
        GetRecurringTasksQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var tasks = await unitOfWork.Tasks.GetRecurringTasksByBoardAsync(request.BoardId, cancellationToken);

        return tasks.Select(GetRecurringTasksQueryResponse.FromEntity).ToList();
    }
}
