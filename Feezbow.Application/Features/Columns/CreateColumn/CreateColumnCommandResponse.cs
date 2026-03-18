using WannabeTrello.Domain.Entities.Common;

namespace WannabeTrello.Application.Features.Columns.CreateColumn;

public record CreateColumnCommandResponse(Result<long> Result);