using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Columns.CreateColumn;

public record CreateColumnCommandResponse(Result<long> Result);