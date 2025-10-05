using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WannabeTrello.Application.Features.Columns.CreateColumn;
using WannabeTrello.Application.Features.Columns.GetColumn;
using WannabeTrello.Domain.Interfaces;
using WannabeTrello.Domain.Interfaces.Services;

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
}