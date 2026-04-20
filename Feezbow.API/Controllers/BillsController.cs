using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Feezbow.Application.Features.Bills.CreateBill;
using Feezbow.Application.Features.Bills.DeleteBill;
using Feezbow.Application.Features.Bills.GetBillsByProject;
using Feezbow.Application.Features.Bills.MarkBillPaid;
using Feezbow.Application.Features.Bills.RecordSplitPayment;
using Feezbow.Application.Features.Bills.SetBillSplit;
using Feezbow.Application.Features.Bills.UpdateBill;

namespace Feezbow.Controllers;

[Authorize(Policy = "EmailConfirmed")]
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/projects/{projectId:long}/bills")]
public class BillsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Creates a new bill for the project, optionally with an equal split across the given users.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(long projectId, [FromBody] CreateBillCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command with { ProjectId = projectId }, cancellationToken));
    }

    /// <summary>
    /// Returns all bills for the project.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByProject(long projectId, [FromQuery] bool includePaid,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetBillsByProjectQuery(projectId, includePaid), cancellationToken));
    }

    /// <summary>
    /// Updates an unpaid bill.
    /// </summary>
    [HttpPut("{billId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long projectId, long billId, [FromBody] UpdateBillCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command with { BillId = billId }, cancellationToken));
    }

    /// <summary>
    /// Sets (or replaces) the split allocation for a bill — equal split or custom shares.
    /// </summary>
    [HttpPut("{billId:long}/split")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetSplit(long projectId, long billId, [FromBody] SetBillSplitCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command with { BillId = billId }, cancellationToken));
    }

    /// <summary>
    /// Records payment of a single member's split share.
    /// </summary>
    [HttpPost("{billId:long}/splits/{userId:long}/pay")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordSplitPayment(long projectId, long billId, long userId,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new RecordSplitPaymentCommand(billId, userId), cancellationToken));
    }

    /// <summary>
    /// Marks the whole bill paid. If recurring, creates the next occurrence.
    /// </summary>
    [HttpPost("{billId:long}/pay")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkPaid(long projectId, long billId, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new MarkBillPaidCommand(billId), cancellationToken));
    }

    /// <summary>
    /// Deletes a bill.
    /// </summary>
    [HttpDelete("{billId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long projectId, long billId, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new DeleteBillCommand(billId), cancellationToken));
    }
}
