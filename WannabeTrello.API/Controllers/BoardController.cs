using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WannabeTrello.Application.Features.Boards.ArchiveBoard;
using WannabeTrello.Application.Features.Boards.CreateBoard;
using WannabeTrello.Application.Features.Boards.GetBoardById;
using WannabeTrello.Application.Features.Boards.RestoreBoard;
using WannabeTrello.Application.Features.Boards.UpdateBoard;

namespace WannabeTrello.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BoardsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Kreira novi board unutar projekta.
    /// </summary>
    /// <param name="command">Komanda koja sadrži detalje boarda.</param>
    /// <returns>ID novoformiranog boarda.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateBoard([FromBody] CreateBoardCommand command)
    {
        var boardId = await mediator.Send(command);
        return CreatedAtAction(nameof(GetBoardById), new { id = boardId }, boardId);
    }

    /// <summary>
    /// Ažurira detalje postojećeg boarda.
    /// </summary>
    /// <param name="id">ID boarda za ažuriranje.</param>
    /// <param name="command">Komanda koja sadrži ažurirane detalje boarda.</param>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateBoard(long id, [FromBody] UpdateBoardCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("ID u URL-u mora se podudarati sa ID-om u telu zahteva.");
        }

        await mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Dohvata specifičan board po ID-u.
    /// </summary>
    /// <param name="id">ID boarda.</param>
    /// <returns>Detalji boarda.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetBoardById(long id) => 
        Ok(await mediator.Send(new GetBoardByIdQuery(id)));
    
    /// <summary>
    /// Arhivira specifičan board po ID-u.
    /// </summary>
    /// <param name="id">ID boarda.</param>
    /// <returns>Resultat akcije.</returns>
    [HttpPost("{id:long}/archive")]
    [ProducesResponseType(typeof(ArchiveBoardCommandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ArchiveBoard(long id) => 
        Ok(await mediator.Send(new ArchiveBoardCommand(id)));
    
    
    /// <summary>
    /// Vraca vec arhivirani board po ID-u.
    /// </summary>
    /// <param name="id">ID boarda.</param>
    /// <returns>Resultat akcije.</returns>
    [HttpPost("{id:long}/restore")]
    [ProducesResponseType(typeof(RestoreBoardCommandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RestoreBoard(long id) =>
        Ok(await mediator.Send(new RestoreBoardCommand(id)));
}