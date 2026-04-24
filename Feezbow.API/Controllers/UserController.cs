using Asp.Versioning;
﻿using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Feezbow.Application.Features.Users.DeactivateUser;
using Feezbow.Application.Features.Users.GetCurrentUserProfile;
using Feezbow.Application.Features.Users.GetUserAssignedTasks;
using Feezbow.Application.Features.Users.GetUserBoards;
using Feezbow.Application.Features.Users.GetUserOwnedProjects;
using Feezbow.Application.Features.Users.GetUserProfile;
using Feezbow.Application.Features.Users.GetUserProjects;
using Feezbow.Application.Features.Users.ReactivateUser;
using Feezbow.Application.Features.Users.UpdateUserProfile;

namespace Feezbow.Controllers
{
    [ApiController]
    [Authorize(Policy = "EmailConfirmed")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UserController(IMediator mediator) : ControllerBase
    {
        [HttpGet("{userId:long}")]
        [ProducesResponseType(typeof(GetUserProfileQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserProfile(long userId)
            => Ok(await mediator.Send(new GetUserProfileQuery(userId)));


        [HttpGet("me")]
        [ProducesResponseType(typeof(GetCurrentUserProfileQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCurrentUserProfile()
            => Ok(await mediator.Send(new GetCurrentUserProfileQuery()));

        [HttpGet("{userId:long}/projects")]
        [ProducesResponseType(typeof(GetUserProjectsQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserProjects(long userId)
            => Ok(await mediator.Send(new GetUserProjectsQuery(userId)));

        [HttpGet("{userId:long}/owned-projects")]
        [ProducesResponseType(typeof(GetUserOwnedProjectsQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserOwnedProjects(long userId)
            => Ok(await mediator.Send(new GetUserOwnedProjectsQuery(userId)));


        [HttpGet("{userId:long}/boards")]
        [ProducesResponseType(typeof(GetUserBoardsQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserBoards(long userId)
            => Ok(await mediator.Send(new GetUserBoardsQuery(userId)));


        [HttpGet("{userId:long}/assigned-tasks")]
        [ProducesResponseType(typeof(GetUserAssignedTasksQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserAssignedTasks(long userId)
            => Ok(await mediator.Send(new GetUserAssignedTasksQuery(userId)));

        [HttpPut("{userId:long}")]
        [ProducesResponseType(typeof(UpdateUserProfileCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUserProfile(long userId, [FromBody] UpdateUserProfileCommand command)
        {
            if (userId != command.UserId)
            {
                return BadRequest();
            }

            return Ok(await mediator.Send(command));
        }

        [HttpPost("{userId:long}/deactivate")]
        [ProducesResponseType(typeof(DeactivateUserCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeactivateUserProfile(long userId)
            => Ok(await mediator.Send(new DeactivateUserCommand(userId)));

        [HttpPost("{userId:long}/reactivate")]
        [ProducesResponseType(typeof(ReactivateUserCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReactivateUserProfile(long userId)
          => Ok(await mediator.Send(new ReactivateUserCommand(userId)));
    }
}
