using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Columns.DeleteColumn;

public record DeleteColumnCommandResponse(Result<long> Result);