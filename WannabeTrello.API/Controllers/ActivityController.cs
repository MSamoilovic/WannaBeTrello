using MediatR;
using Microsoft.AspNetCore.Mvc;
using WannabeTrello.Application.Features.Activities.GetActivityByBoard;
using WannabeTrello.Application.Features.Activities.GetActivityByProject;
using WannabeTrello.Application.Features.Activities.GetActivityByTask;
using WannabeTrello.Application.Features.Activities.GetActivityByUser;

namespace WannabeTrello.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivityController(IMediator mediator): ControllerBase
{

    [HttpGet("boards/{boardId:long}")]
    [ProducesResponseType(typeof(IReadOnlyList<GetActivityByBoardQueryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetActivityByBoard(long boardId)
        => Ok(await mediator.Send(new GetActivityByBoardQuery(boardId)));

    [HttpGet("projects/{projectId:long}")]
    [ProducesResponseType(typeof(IReadOnlyList<GetActivityByProjectQueryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetActivityByProject(long projectId)
       => Ok(await mediator.Send(new GetActivityByProjectQuery(projectId)));

    [HttpGet("tasks/{taskId:long}")]
    [ProducesResponseType(typeof(IReadOnlyList<GetActivityByTaskQueryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetActivityByTask(long taskId)
        => Ok(await mediator.Send(new GetActivityByTaskQuery(taskId)));

    [HttpGet("users/{userId:long}")]
    [ProducesResponseType(typeof(IReadOnlyList<GetActivityByUserQueryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetActivityByUser(long userId)
        => Ok(await mediator.Send(new GetActivityByUserQuery(userId)));

}
