using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Feezbow.Application.Features.Users.SearchUsers;

namespace Feezbow.Controllers.OData
{
    [ApiController]
    [Route("api/Users")]
    public class UserController(IMediator mediator): ODataController
    {
        [HttpGet("search")]
        [EnableQuery(PageSize = 50)]
        [ProducesResponseType(typeof(IQueryable<SearchUsersQueryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchUsers()
            => Ok(await mediator.Send(new SearchUsersQuery()));
    }
}
