using MediatR;

namespace WannabeTrello.Application.Features.Columns.GetColumn;

public record GetColumnByIdQuery(long ColumnId): IRequest<GetColumnByIdQueryResponse>;