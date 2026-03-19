using System.Collections.Immutable;
using MediatR;

namespace Feezbow.Application.Features.Tasks.GetTasksByBoardId;

public class GetTasksByBoardIdQuery: IRequest<ImmutableList<GetTaskByBoardIdQueryResponse>>
{
    public long BoardId { get; set; }   
}