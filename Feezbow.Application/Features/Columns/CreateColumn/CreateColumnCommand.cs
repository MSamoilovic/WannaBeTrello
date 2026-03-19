using MediatR;

namespace Feezbow.Application.Features.Columns.CreateColumn;

public class CreateColumnCommand: IRequest<CreateColumnCommandResponse>
{
    public long BoardId { get; set; }
    public string? Name { get; set; }
    public int Order { get; set; }
}