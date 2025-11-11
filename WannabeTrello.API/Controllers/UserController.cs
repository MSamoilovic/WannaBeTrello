using MediatR;
using Microsoft.AspNetCore.Mvc;
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
    }
}
