using WannabeTrello.Domain.Entities.Common;

namespace WannabeTrello.Application.Features.Boards.CreateBoard;

public record CreateBoardCommandResponse(Result<long> Result);