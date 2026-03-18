using MediatR;

namespace WannabeTrello.Application.Features.Tasks.GetCommentsByTaskId;

public record GetCommentsByTaskIdCommand(long TaskId): IRequest<IReadOnlyList<GetCommentsByTaskIdCommandResponse>>;