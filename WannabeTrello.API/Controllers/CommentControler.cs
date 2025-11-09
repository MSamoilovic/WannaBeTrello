using MediatR;
using Microsoft.AspNetCore.Mvc;
using WannabeTrello.Application.Features.Comments.DeleteComment;

namespace WannabeTrello.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentControler(IMediator mediator): ControllerBase
{
    [HttpPut("{commentId:long}/delete")]
    [ProducesResponseType(typeof(DeleteCommentCommandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteComment(long commentId) =>
        Ok(await mediator.Send(new DeleteCommentCommand(commentId)));
}