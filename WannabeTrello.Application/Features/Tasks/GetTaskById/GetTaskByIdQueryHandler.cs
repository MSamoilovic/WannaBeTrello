using MediatR;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Application.Features.Tasks.GetTaskById;

public class GetTaskByIdQueryHandler(
    IBoardTaskRepository taskRepository)
    : IRequestHandler<GetTaskByIdQuery, GetTaskByIdQueryResponse>
{
    public async Task<GetTaskByIdQueryResponse> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(request.Id);

        if (task == null)
        {
            throw new NotFoundException(nameof(BoardTask), request.Id);
        }
        
        return GetTaskByIdQueryResponse.FromEntity(task);
    }
}