using MediatR;

namespace Feezbow.Application.Features.Boards.ArchiveBoard;

public record ArchiveBoardCommand(long BoardId): IRequest<ArchiveBoardCommandResponse>;
