using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Columns.ReorderColumn;

public record ReorderColumnCommandResponse(Result<long> Result);