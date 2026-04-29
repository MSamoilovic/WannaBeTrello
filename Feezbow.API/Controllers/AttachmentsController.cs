using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Feezbow.Application.Features.Attachments.DeleteAttachment;
using Feezbow.Application.Features.Attachments.DownloadAttachment;
using Feezbow.Application.Features.Attachments.GetAttachmentById;
using Feezbow.Application.Features.Attachments.GetAttachmentsByOwner;
using Feezbow.Application.Features.Attachments.UploadAttachment;
using Feezbow.Domain.Enums;

namespace Feezbow.Controllers;

[Authorize(Policy = "EmailConfirmed")]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AttachmentsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Uploads a file and links it to the given owner (Bill or Household).
    /// Multipart/form-data: <c>file</c> (required), <c>ownerType</c>, <c>ownerId</c>, optional <c>description</c>.
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Upload(
        [FromForm] AttachmentOwnerType ownerType,
        [FromForm] long ownerId,
        [FromForm] IFormFile file,
        [FromForm] string? description,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "File is required." });

        await using var stream = file.OpenReadStream();
        var command = new UploadAttachmentCommand(
            ownerType,
            ownerId,
            file.FileName,
            file.ContentType,
            file.Length,
            stream,
            description);

        return Ok(await mediator.Send(command, cancellationToken));
    }

    /// <summary>
    /// Lists attachments for the given owner.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByOwner(
        [FromQuery] AttachmentOwnerType ownerType,
        [FromQuery] long ownerId,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetAttachmentsByOwnerQuery(ownerType, ownerId), cancellationToken));
    }

    /// <summary>
    /// Returns metadata for a single attachment.
    /// </summary>
    [HttpGet("{attachmentId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long attachmentId, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetAttachmentByIdQuery(attachmentId), cancellationToken));
    }

    /// <summary>
    /// Streams the attachment's binary content with the original file name and content type.
    /// </summary>
    [HttpGet("{attachmentId:long}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(long attachmentId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DownloadAttachmentQuery(attachmentId), cancellationToken);
        return File(result.Content, result.ContentType, result.FileName);
    }

    /// <summary>
    /// Deletes an attachment (metadata + file).
    /// </summary>
    [HttpDelete("{attachmentId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long attachmentId, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new DeleteAttachmentCommand(attachmentId), cancellationToken));
    }
}
