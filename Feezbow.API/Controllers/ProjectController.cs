using Asp.Versioning;
﻿using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Feezbow.Application.Features.Projects.GetBoardsByProjectId;
using Feezbow.Application.Features.Projects.AddProjectMember;
using Feezbow.Application.Features.Projects.ArchiveProject;
using Feezbow.Application.Features.Projects.UnarchiveProject;
using Feezbow.Application.Features.Projects.CreateProject;
using Feezbow.Application.Features.Projects.GetProjectById;
using Feezbow.Application.Features.Projects.GetProjectMembersById;
using Feezbow.Application.Features.Projects.RemoveProjectMember;
using Feezbow.Application.Features.Projects.UpdateProject;
using Feezbow.Application.Features.Projects.UpdateProjectMemberRole;

namespace Feezbow.Controllers;

[Authorize(Policy = "EmailConfirmed")]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProjectController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Creates a new project within the application.
    /// </summary>
    /// <param name="command">Command that contains details for a project</param>
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

    [HttpPost("{id}/archive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ArchiveProject(long id)
    {
        var response = await mediator.Send(new ArchiveProjectCommand(id));
        return Ok(response);
    }

    [HttpPost("{id}/restore")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UnarchiveProject(long id)
    {
        var response = await mediator.Send(new UnarchiveProjectCommand(id));
        return Ok(response);
    }

    [HttpPost("{id:long}/add-member")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddProjectMember(long id, [FromBody] AddProjectMemberCommand command)
    {
        if (id != command.ProjectId)
            return BadRequest("Bad Id in request");

        return Ok(await mediator.Send(command));
    }

    [HttpGet("{id:long}/members")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProjectMembersBy(long id)
    {
        return Ok(await mediator.Send(new GetProjectMembersByIdQuery(id)));
    }

    [HttpDelete("{id:long}/members/{memberId:long}")]
    [ProducesResponseType(typeof(RemoveProjectMemberCommandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveProjectMember(long id, long memberId)
        => Ok(await mediator.Send(new RemoveProjectMemberCommand(id, memberId)));

    [HttpPut("{id:long}/members/{memberId:long}")]
    [ProducesResponseType(typeof(UpdateProjectMemberRoleCommandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProjectMemberRole(long id, long memberId,
        [FromBody] UpdateProjectMemberRoleCommand command)
    {
        if (id != command.ProjectId)
            return BadRequest("Bad ProjectId in request");

        if (memberId != command.MemberId)
        {
            return BadRequest("Bad MemberId in request");
        }
        
        return Ok(await mediator.Send(command));
    }
    
    [HttpGet("{id:long}/boards")]
    [ProducesResponseType(typeof(List<GetBoardsByProjectIdQueryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBoardsByProjectId(long id)
    {
        var response = await mediator.Send(new GetBoardsByProjectIdQuery(id));
        return Ok(response);
    }
}