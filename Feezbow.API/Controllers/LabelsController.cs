using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Feezbow.Application.Features.Labels.AddLabelToTask;
using Feezbow.Application.Features.Labels.CreateLabel;
using Feezbow.Application.Features.Labels.DeleteLabel;
using Feezbow.Application.Features.Labels.GetLabelsByBoard;
using Feezbow.Application.Features.Labels.RemoveLabelFromTask;
using Feezbow.Application.Features.Labels.UpdateLabel;

namespace Feezbow.Controllers;

[Authorize(Policy = "EmailConfirmed")]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class LabelsController(IMediator mediator) : ControllerBase
{
    [HttpGet("board/{boardId:long}")]
    [ProducesResponseType(typeof(IReadOnlyList<GetLabelsByBoardQueryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByBoard(long boardId)
        => Ok(await mediator.Send(new GetLabelsByBoardQuery(boardId)));

    [HttpPost]
    [ProducesResponseType(typeof(CreateLabelCommandResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateLabelCommand command)
    {
        var response = await mediator.Send(command);
        return CreatedAtAction(nameof(GetByBoard), new { boardId = response.BoardId }, response);
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(UpdateLabelCommandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateLabelCommand command)
    {
        if (id != command.LabelId)
            return BadRequest("Id in URL does not match Id in body.");

        return Ok(await mediator.Send(command));
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(typeof(DeleteLabelCommandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id)
        => Ok(await mediator.Send(new DeleteLabelCommand(id)));

    [HttpPost("tasks/{taskId:long}/labels/{labelId:long}")]
    [ProducesResponseType(typeof(AddLabelToTaskCommandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddToTask(long taskId, long labelId)
        => Ok(await mediator.Send(new AddLabelToTaskCommand(taskId, labelId)));

    [HttpDelete("tasks/{taskId:long}/labels/{labelId:long}")]
    [ProducesResponseType(typeof(RemoveLabelFromTaskCommandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFromTask(long taskId, long labelId)
        => Ok(await mediator.Send(new RemoveLabelFromTaskCommand(taskId, labelId)));
}
