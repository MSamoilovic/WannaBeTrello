using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Repositories;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Features.Tasks.GetTaskById;

public class GetTaskByIdQueryHandler(
    IBoardTaskService taskService, ICurrentUserService currentUserService)
    : IRequestHandler<GetTaskByIdQuery, GetTaskByIdQueryResponse>
{
    public async Task<GetTaskByIdQueryResponse> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        if (currentUserService.UserId is null)
            throw new UnauthorizedAccessException("User is not authenticated");
        
        var task = await taskService.GetTaskByIdAsync(request.taskId, currentUserService.UserId.Value, cancellationToken);
        
        return GetTaskByIdQueryResponse.FromEntity(task!);
    }
}