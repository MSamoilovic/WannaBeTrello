using MediatR;

namespace Feezbow.Application.Features.Columns.DeleteColumn;

public record DeleteColumnCommand(long ColumnId) : IRequest<DeleteColumnCommandResponse>;