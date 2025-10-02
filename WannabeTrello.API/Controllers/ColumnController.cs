using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WannabeTrello.Application.Features.Columns.CreateColumn;
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateColumn([FromBody] CreateColumnCommand command)
    {
        var response = await mediator.Send(command);
        return Ok(response);
    }
}