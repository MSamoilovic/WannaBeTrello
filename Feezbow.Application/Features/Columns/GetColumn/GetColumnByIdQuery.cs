using MediatR;

namespace Feezbow.Application.Features.Columns.GetColumn;

public record GetColumnByIdQuery(long ColumnId): IRequest<GetColumnByIdQueryResponse>;