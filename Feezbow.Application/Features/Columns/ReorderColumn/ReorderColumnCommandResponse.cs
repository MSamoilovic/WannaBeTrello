using WannabeTrello.Domain.Entities.Common;

namespace WannabeTrello.Application.Features.Columns.ReorderColumn;

public record ReorderColumnCommandResponse(Result<long> Result);