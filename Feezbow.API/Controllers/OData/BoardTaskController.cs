using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using WannabeTrello.Application.Features.Tasks.SearchTasks;

namespace WannabeTrello.Controllers.OData
{
    [ApiController]
    [Route("api/Tasks")]
    public class TasksController(IMediator mediator) : ODataController
    {

        [HttpGet("search")]
        [EnableQuery(PageSize = 50)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IQueryable<SearchTaskQueryResponse>>> SearchTasksOData()
            => Ok(await mediator.Send(new SearchTaskQuery()));
    }
}
