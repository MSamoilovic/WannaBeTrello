using MediatR;

namespace WannabeTrello.Application.Features.Columns.DeleteColumn;

public record DeleteColumnCommand(long ColumnId) : IRequest<DeleteColumnCommandResponse>;