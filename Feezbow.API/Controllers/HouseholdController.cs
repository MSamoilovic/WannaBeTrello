using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Feezbow.Application.Features.Household.AssignHouseholdRole;
using Feezbow.Application.Features.Household.CreateHouseholdProfile;
using Feezbow.Application.Features.Household.GetHouseholdMembers;
using Feezbow.Application.Features.Household.GetHouseholdProfile;
using Feezbow.Application.Features.Household.RemoveHouseholdRole;
using Feezbow.Application.Features.Household.UpdateHouseholdProfile;

namespace Feezbow.Controllers;

[Authorize(Policy = "EmailConfirmed")]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class HouseholdController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Creates a household profile for an existing project.
    /// </summary>
    [HttpPost("{projectId:long}/profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateProfile(long projectId, [FromBody] CreateHouseholdProfileCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command with { ProjectId = projectId }, cancellationToken));
    }

    /// <summary>
    /// Returns the household profile for a project.
    /// </summary>
    [HttpGet("{projectId:long}/profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(long projectId, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetHouseholdProfileQuery(projectId), cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Updates the household profile for a project.
    /// </summary>
    [HttpPut("{projectId:long}/profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile(long projectId, [FromBody] UpdateHouseholdProfileCommand command,
        CancellationToken cancellationToken)
    {
        var response = await mediator.Send(command with { ProjectId = projectId }, cancellationToken);
        return Ok(response.Result);
    }

    /// <summary>
    /// Returns all members of the household with their roles.
    /// </summary>
    [HttpGet("{projectId:long}/members")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMembers(long projectId, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetHouseholdMembersQuery(projectId), cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Assigns a household role to a project member.
    /// </summary>
    [HttpPut("{projectId:long}/members/{memberId:long}/role")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRole(long projectId, long memberId,
        [FromBody] AssignHouseholdRoleCommand command, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(command with { ProjectId = projectId, MemberId = memberId }, cancellationToken);
        return Ok(response.Result);
    }

    /// <summary>
    /// Removes the household role from a project member.
    /// </summary>
    [HttpDelete("{projectId:long}/members/{memberId:long}/role")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveRole(long projectId, long memberId, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new RemoveHouseholdRoleCommand(projectId, memberId), cancellationToken);
        return Ok(response.Result);
    }
}
