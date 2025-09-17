using MediatR;

namespace WannabeTrello.Application.Features.Boards.ArchiveBoard;

public record ArchiveBoardCommand(long BoardId): IRequest<ArchiveBoardCommandResponse>;
