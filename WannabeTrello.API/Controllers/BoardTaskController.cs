﻿using MediatR;
using Microsoft.AspNetCore.Mvc;
using WannabeTrello.Application.Features.Tasks.AddCommentToTask;
using WannabeTrello.Application.Features.Tasks.CreateTask;
using WannabeTrello.Application.Features.Tasks.GetTaskById;
using WannabeTrello.Application.Features.Tasks.GetTasksByBoardId;
using WannabeTrello.Application.Features.Tasks.MoveTask;


namespace WannabeTrello.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TasksController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Kreira novi zadatak u određenoj koloni.
        /// </summary>
        /// <param name="command">Komanda koja sadrži detalje zadatka.</param>
        /// <returns>ID novoformiranog zadatka.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskCommand command)
        {
            var taskId = await _mediator.Send(command);
            // U idealnom scenariju, ovde bi bio GetTaskByIdQuery za CreatedAtAction
            return CreatedAtAction(nameof(MoveTask), new { id = taskId },
                taskId); // Koristimo MoveTask kao placeholder za rute
        }

        /// <summary>
        /// Premesta zadatak iz jedne kolone u drugu na boardu.
        /// </summary>
        /// <param name="id">ID zadatka za premeštanje.</param>
        /// <param name="command">Komanda koja sadrži ID nove kolone.</param>
        [HttpPut("{id}/move")]
        [ProducesResponseType(StatusCodes.Status204NoContent)] // Uspešno ažuriranje, bez sadržaja
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MoveTask(long id, [FromBody] MoveTaskCommand command)
        {
            if (id != command.TaskId) // Osiguraj da se ID u URL-u podudara sa ID-om u telu
            {
                return BadRequest("ID zadatka u URL-u mora se podudarati sa ID-om zadatka u telu zahteva.");
            }

            await _mediator.Send(command);
            return NoContent();
        }
        

        /// <summary>
        /// Dodaje komentar postojećem zadatku.
        /// </summary>
        /// <param name="taskId">ID zadatka za dodavanje komentara.</param>
        /// <param name="command">Komanda koja sadrži sadržaj komentara.</param>
        [HttpPost("{taskId:guid}/comments")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddComment(long taskId, [FromBody] AddCommentToTaskCommand command)
        {
            if (taskId != command.TaskId)
            {
                return BadRequest("ID zadatka u URL-u mora se podudarati sa ID-om zadatka u telu zahteva.");
            }
            await _mediator.Send(command);
            return StatusCode(StatusCodes.Status201Created); 
        }
        
        /// Dohvata task po ID-u.
        /// </summary>
        /// <param name="id">ID zadatka.</param>
        /// <returns>Detalji zadatka.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetTaskById(long id)
        {
            var query = new GetTaskByIdQuery { Id = id };
            var task = await _mediator.Send(query);
            return Ok(task);
        }

        /// <summary>
        /// Dohvata sve taskove za specifičan board po ID-u boarda.
        /// </summary>
        /// <param name="boardId">ID boarda.</param>
        /// <returns>Lista zadataka.</returns>
        [HttpGet("board/{boardId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetTasksByBoardId(long boardId)
        {
            var query = new GetTasksByBoardIdQuery { BoardId = boardId };
            var tasks = await _mediator.Send(query);
            return Ok(tasks);
        }
        
    }
}