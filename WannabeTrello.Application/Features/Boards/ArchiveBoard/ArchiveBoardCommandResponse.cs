using WannabeTrello.Domain.Entities.Common;

namespace WannabeTrello.Application.Features.Boards.ArchiveBoard;

public record ArchiveBoardCommandResponse(Result<long> Result);
