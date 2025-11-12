using MediatR;
using Microsoft.AspNetCore.Mvc;
using WannabeTrello.Application.Features.Users.DeactivateUser;
using WannabeTrello.Application.Features.Users.GetCurrentUserProfile;
using WannabeTrello.Application.Features.Users.GetUserProfile;
using WannabeTrello.Application.Features.Users.UpdateUserProfile;

namespace WannabeTrello.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(IMediator mediator) : ControllerBase
    {
        [HttpGet("{userId:long")]
        [ProducesResponseType(typeof(GetUserProfileQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserProfile(long userId)
            => Ok(await mediator.Send(new GetUserProfileQuery(userId)));


        [HttpGet("me")]
        [ProducesResponseType(typeof(GetCurrentUserProfileQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCurrentUserProfile()
            => Ok(await mediator.Send(new GetCurrentUserProfileQuery()));


        [HttpPut("{userId: long}")]
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

        [HttpPost("{userId: long}/deactivate")]
        [ProducesResponseType(typeof(DeactivateUserCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeactivateUserProfile(long userId)
            => Ok(await mediator.Send(new DeactivateUserCommand(userId)));
    }
}
