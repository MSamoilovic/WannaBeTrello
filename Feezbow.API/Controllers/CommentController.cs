using Asp.Versioning;
﻿using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Feezbow.Application.Features.Comments.DeleteComment;
using Feezbow.Application.Features.Comments.RestoreComment;
using Feezbow.Application.Features.Comments.UpdateCommentContent;

namespace Feezbow.Controllers;

[ApiController]
[Authorize(Policy = "EmailConfirmed")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CommentsController(IMediator mediator): ControllerBase
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