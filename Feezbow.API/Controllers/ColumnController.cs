using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WannabeTrello.Application.Features.Columns.CreateColumn;
using WannabeTrello.Application.Features.Columns.DeleteColumn;
using WannabeTrello.Application.Features.Columns.GetColumn;
using WannabeTrello.Application.Features.Columns.UpdateColumn;

namespace WannabeTrello.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ColumnController(IMediator mediator): ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CreateColumnCommandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateColumn([FromBody] CreateColumnCommand command) =>
        Ok(await mediator.Send(command));
    
    
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(GetColumnByIdQueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetColumnById(long id) => 
        Ok(await mediator.Send(new GetColumnByIdQuery(id)));
    
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(UpdateColumnCommandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateColumn(long id, [FromBody] UpdateColumnCommand updateColumn)
    {
        if (id != updateColumn.ColumnId)
            return BadRequest("Id in URL does not match Id in body");

        return Ok(await mediator.Send(updateColumn));
    }
    
    [HttpDelete("{id:long}")]
    [ProducesResponseType(typeof(DeleteColumnCommandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteColumn(long id) => 
        Ok(await mediator.Send(new DeleteColumnCommand(id)));
}