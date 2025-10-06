using MediatR;

namespace WannabeTrello.Application.Features.Columns.UpdateColumn;

public record UpdateColumnCommand(long ColumnId, string? NewName, int? WipLimit): IRequest<UpdateColumnCommandResponse>;
