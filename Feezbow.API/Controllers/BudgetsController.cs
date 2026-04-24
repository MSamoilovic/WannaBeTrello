using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Feezbow.Application.Features.Budgets.GetProjectBudgetSummary;
using Feezbow.Application.Features.Budgets.GetProjectBudgetTimeline;
using Feezbow.Application.Features.Budgets.GetUserBudgetSummary;

namespace Feezbow.Controllers;

[Authorize(Policy = "EmailConfirmed")]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class BudgetsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Returns an aggregated budget summary for the project: totals by currency, category, and member,
    /// plus upcoming and overdue bills. Defaults to the current calendar month if from/to are omitted.
    /// </summary>
    [HttpGet("projects/{projectId:long}/summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProjectSummary(
        long projectId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int upcomingDays = 30,
        CancellationToken cancellationToken = default)
    {
        return Ok(await mediator.Send(
            new GetProjectBudgetSummaryQuery(projectId, from, to, upcomingDays), cancellationToken));
    }

    /// <summary>
    /// Returns the current user's aggregated budget across all projects they belong to within the given range.
    /// Defaults to the current calendar month if from/to are omitted.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyBudget(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken = default)
    {
        return Ok(await mediator.Send(new GetUserBudgetSummaryQuery(from, to), cancellationToken));
    }

    /// <summary>
    /// Returns per-month totals for a project's bills, grouped by (year, month, currency).
    /// Defaults to the trailing 12 months if from/to are omitted.
    /// </summary>
    [HttpGet("projects/{projectId:long}/timeline")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProjectTimeline(
        long projectId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int months = 12,
        CancellationToken cancellationToken = default)
    {
        return Ok(await mediator.Send(
            new GetProjectBudgetTimelineQuery(projectId, from, to, months), cancellationToken));
    }
}
