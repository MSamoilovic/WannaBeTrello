using WannabeTrello.Domain.Entities.Common;

namespace WannabeTrello.Application.Features.Boards.RestoreBoard;

public record RestoreBoardCommandResponse(Result<long> Result);