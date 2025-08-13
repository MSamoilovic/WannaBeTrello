using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WannabeTrello.Application.Features.Projects.ArchiveProject;
using WannabeTrello.Application.Features.Projects.CreateProject;
using WannabeTrello.Application.Features.Projects.GetProjectById;
using WannabeTrello.Application.Features.Projects.UpdateProject;

namespace WannabeTrello.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProjectController(IMediator mediator) : ControllerBase
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

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProject(int id, [FromBody] UpdateProjectCommand command)
    {
        if (id != command.ProjectId)
        {
            return BadRequest("ID u URL-u mora se podudarati sa ID-om u telu zahteva.");
        }
        
        return Ok(await mediator.Send(command));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProjectById(long id)
    {
        var response = await mediator.Send(new GetProjectByIdQuery(id));
        return Ok(response);
    }

    [HttpPost("/archive/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ArchiveProject(long id)
    {
        var response = await mediator.Send(new ArchiveProjectCommand(id));
        return Ok(response);
    }
}