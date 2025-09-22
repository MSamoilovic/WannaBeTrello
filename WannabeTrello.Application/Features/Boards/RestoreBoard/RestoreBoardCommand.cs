using MediatR;

namespace WannabeTrello.Application.Features.Boards.RestoreBoard;

public record RestoreBoardCommand(long BoardId): IRequest<RestoreBoardCommandResponse>;