using MediatR;

namespace Feezbow.Application.Features.Columns.UpdateColumn;

public record UpdateColumnCommand(long ColumnId, string? NewName, int? WipLimit): IRequest<UpdateColumnCommandResponse>;
