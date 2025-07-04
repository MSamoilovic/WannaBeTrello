using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using WannabeTrello.Application.Features.Tasks.SearchTasks;

namespace WannabeTrello.Controllers.OData
{
    [ApiController]
    [Route("api/Tasks")]
    public class TasksController : ODataController
    {
        private readonly IMediator _mediator;
       
        public TasksController(IMediator mediator)
        {
            _mediator = mediator;
        }

     
        /// <summary>
        /// Omogućava pretragu i filtriranje zadataka sa podrškom za OData upite.
        /// Klijent može da koristi OData upitne parametre kao što su $filter, $orderby, $top, $skip.
        /// Primedba: Autorizacija se vrši u rukovaocu da bi se prikazali samo zadaci kojima korisnik ima pristup.
        /// </summary>
        /// <returns>IQueryable lista zadataka koja podržava OData upite.</returns>
        [HttpGet("search")]
        [EnableQuery(PageSize = 50)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IQueryable<SearchTaskQueryResponse>>> SearchTasksOData()
        {
            var query = new SearchTaskQuery();
            var tasks = await _mediator.Send(query);
            return Ok(tasks);
        }
    }
}
