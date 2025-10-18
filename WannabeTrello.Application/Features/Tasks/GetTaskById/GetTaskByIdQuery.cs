using MediatR;

namespace WannabeTrello.Application.Features.Tasks.GetTaskById;

public record GetTaskByIdQuery(long taskId): IRequest<GetTaskByIdQueryResponse>;