using MediatR;

namespace Feezbow.Application.Features.Tasks.GetTaskById;

public record GetTaskByIdQuery(long taskId): IRequest<GetTaskByIdQueryResponse>;