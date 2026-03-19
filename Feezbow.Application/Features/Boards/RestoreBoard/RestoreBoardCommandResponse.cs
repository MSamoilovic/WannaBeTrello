using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Boards.RestoreBoard;

public record RestoreBoardCommandResponse(Result<long> Result);