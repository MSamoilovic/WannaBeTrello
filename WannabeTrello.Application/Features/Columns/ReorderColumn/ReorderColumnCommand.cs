using MediatR;

namespace WannabeTrello.Application.Features.Columns.ReorderColumn;

public class ReorderColumnCommand: IRequest<ReorderColumnCommandResponse>
{
    public long BoardId { get; set; }
    public Dictionary<long, int> ColumnOrders { get; set; } = [];
}