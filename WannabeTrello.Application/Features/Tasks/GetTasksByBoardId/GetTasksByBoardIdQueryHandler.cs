using System.Collections.Immutable;
using MediatR;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Boards.GetBoardById;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Application.Features.Tasks.GetTasksByBoardId;

public class GetTasksByBoardIdQueryHandler(
    IBoardRepository boardRepository,
    IColumnRepository columnRepository,
    IBoardTaskRepository taskRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetTasksByBoardIdQuery, ImmutableList<GetTaskByBoardIdQueryResponse>>
{
    public async Task<ImmutableList<GetTaskByBoardIdQueryResponse>> Handle(GetTasksByBoardIdQuery request, CancellationToken cancellationToken)
    {
        var board = await boardRepository.GetBoardWithDetailsAsync(request.BoardId);
        if (board == null)
        {
            throw new NotFoundException(nameof(Domain.Entities.Board), request.BoardId);
        }

        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue || board.BoardMembers.All(bm => bm.UserId != currentUserService.UserId.Value))
        {
            throw new AccessDeniedException("Nemate pristup za pregled zadataka na ovom boardu.");
        }

        var allTasks = new List<GetTaskByBoardIdQueryResponse>();

        var columns = new List<Column>();

        foreach (var column in columns)
        {
            var tasksInColumn = await taskRepository.GetTasksByColumnIdAsync(column.Id);
            allTasks.AddRange(tasksInColumn.Select(GetTaskByBoardIdQueryResponse.FromEntity));
        }

        return allTasks.ToImmutableList();
    }
}