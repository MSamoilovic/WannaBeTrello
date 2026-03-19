using MediatR;

namespace Feezbow.Application.Features.Boards.RestoreBoard;

public record RestoreBoardCommand(long BoardId): IRequest<RestoreBoardCommandResponse>;