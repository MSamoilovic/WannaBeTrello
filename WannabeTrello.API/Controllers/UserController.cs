using MediatR;
using Microsoft.AspNetCore.Mvc;
using WannabeTrello.Application.Features.Users.GetCurrentUserProfile;
using WannabeTrello.Application.Features.Users.GetUserProfile;

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
    }
}
