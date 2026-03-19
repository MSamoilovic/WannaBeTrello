using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Boards.CreateBoard;

public record CreateBoardCommandResponse(Result<long> Result);