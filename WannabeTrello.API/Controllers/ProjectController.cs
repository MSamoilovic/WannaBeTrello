using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WannabeTrello.Application.Features.Projects.CreateProject;

namespace WannabeTrello.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProjectController(IMediator mediator): ControllerBase
{
    /// <summary>
    /// Creates a new project within the application.
    /// </summary>
    /// <param name="command">Command that contains details for project</param>
    /// <returns>ID of newly created board</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectCommand command)
    {
        var projectId = await mediator.Send(command);
        return CreatedAtAction(nameof(CreateProject), new { id = projectId }, projectId);
    }
}