using MediatR;

namespace Feezbow.Application.Features.Tasks.GetCommentsByTaskId;

public record GetCommentsByTaskIdCommand(long TaskId): IRequest<IReadOnlyList<GetCommentsByTaskIdCommandResponse>>;