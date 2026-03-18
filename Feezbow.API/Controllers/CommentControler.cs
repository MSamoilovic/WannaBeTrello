using MediatR;
using Microsoft.AspNetCore.Mvc;
using WannabeTrello.Application.Features.Comments.DeleteComment;
using WannabeTrello.Application.Features.Comments.RestoreComment;
using WannabeTrello.Application.Features.Comments.UpdateCommentContent;

namespace WannabeTrello.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentControler(IMediator mediator): ControllerBase
{
    [HttpPut("{commentId:long}")]
    [ProducesResponseType(typeof(UpdateCommentContentCommandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateCommentContent(long commentId,
        [FromBody] UpdateCommentContentCommand command)
    {
        if (command.CommentId != commentId)
        {
            return BadRequest();
        }
        
        return Ok(await mediator.Send(command));
    }
    
    [HttpPut("{commentId:long}/delete")]
    [ProducesResponseType(typeof(DeleteCommentCommandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteComment(long commentId) =>
        Ok(await mediator.Send(new DeleteCommentCommand(commentId)));
    
    [HttpPut("{commentId:long}/restore")]
    [ProducesResponseType(typeof(RestoreCommentCommandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RestoreComment(long commentId) =>
        Ok(await mediator.Send(new RestoreCommentCommand(commentId)));
}