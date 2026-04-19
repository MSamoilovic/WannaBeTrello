using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Feezbow.Application.Features.Chores.AssignChore;
using Feezbow.Application.Features.Chores.CompleteChore;
using Feezbow.Application.Features.Chores.CreateChore;
using Feezbow.Application.Features.Chores.DeleteChore;
using Feezbow.Application.Features.Chores.GetChoresByProject;
using Feezbow.Application.Features.Chores.UpdateChore;

namespace Feezbow.Controllers;

[Authorize(Policy = "EmailConfirmed")]
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/projects/{projectId:long}/chores")]
public class ChoresController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Creates a new household chore for the project.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(long projectId, [FromBody] CreateChoreCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command with { ProjectId = projectId }, cancellationToken));
    }

    /// <summary>
    /// Returns all chores for the project.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByProject(long projectId, [FromQuery] bool includeCompleted,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetChoresByProjectQuery(projectId, includeCompleted), cancellationToken));
    }

    /// <summary>
    /// Updates a household chore.
    /// </summary>
    [HttpPut("{choreId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long projectId, long choreId, [FromBody] UpdateChoreCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command with { ChoreId = choreId }, cancellationToken));
    }

    /// <summary>
    /// Marks a chore as completed. If recurring, creates the next occurrence.
    /// </summary>
    [HttpPost("{choreId:long}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(long projectId, long choreId, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new CompleteChoreCommand(choreId), cancellationToken));
    }

    /// <summary>
    /// Assigns or unassigns a chore to a project member.
    /// </summary>
    [HttpPut("{choreId:long}/assign")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Assign(long projectId, long choreId, [FromBody] AssignChoreCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command with { ChoreId = choreId }, cancellationToken));
    }

    /// <summary>
    /// Deletes a household chore.
    /// </summary>
    [HttpDelete("{choreId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long projectId, long choreId, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new DeleteChoreCommand(choreId), cancellationToken));
    }
}
