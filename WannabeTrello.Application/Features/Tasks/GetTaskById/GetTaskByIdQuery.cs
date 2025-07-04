using MediatR;

namespace WannabeTrello.Application.Features.Tasks.GetTaskById;

public class GetTaskByIdQuery : IRequest<GetTaskByIdQueryResponse>
{
    public long Id { get; set; }
}