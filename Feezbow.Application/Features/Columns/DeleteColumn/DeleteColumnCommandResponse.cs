using WannabeTrello.Domain.Entities.Common;

namespace WannabeTrello.Application.Features.Columns.DeleteColumn;

public record DeleteColumnCommandResponse(Result<long> Result);