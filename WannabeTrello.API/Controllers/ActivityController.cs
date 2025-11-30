using MediatR;
using Microsoft.AspNetCore.Mvc;
using WannabeTrello.Application.Features.Activities.GetActivityByBoard;

namespace WannabeTrello.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivityController(IMediator mediator): ControllerBase
{

    [HttpGet("boards/{boardId: long}")]
    [ProducesResponseType(typeof(IReadOnlyList<GetActivityByBoardQueryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetActivityByBoard(long boardId)
        => Ok(await mediator.Send(new GetActivityByBoardQuery(boardId)));
}
