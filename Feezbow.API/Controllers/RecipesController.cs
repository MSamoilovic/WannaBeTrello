using Asp.Versioning;
using Feezbow.Application.Features.Recipes.CreateRecipe;
using Feezbow.Application.Features.Recipes.DeleteRecipe;
using Feezbow.Application.Features.Recipes.GetRecipeById;
using Feezbow.Application.Features.Recipes.GetRecipesByProject;
using Feezbow.Application.Features.Recipes.ParseRecipe;
using Feezbow.Application.Features.Recipes.ReplaceRecipeIngredients;
using Feezbow.Application.Features.Recipes.UpdateRecipe;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feezbow.Controllers;

[Authorize(Policy = "EmailConfirmed")]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class RecipesController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Creates a recipe in the given project, optionally with an initial ingredient list.
    /// </summary>
    [HttpPost("projects/{projectId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        long projectId,
        [FromBody] CreateRecipeCommand command,
        CancellationToken cancellationToken
    )
    {
        return Ok(await mediator.Send(command with { ProjectId = projectId }, cancellationToken));
    }

    /// <summary>Returns a paged list of recipes for the project (default 50 per page).</summary>
    [HttpGet("projects/{projectId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByProject(
        long projectId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default
    )
    {
        return Ok(
            await mediator.Send(
                new GetRecipesByProjectQuery(projectId, skip, take),
                cancellationToken
            )
        );
    }

    /// <summary>Returns a single recipe with ingredients.</summary>
    [HttpGet("{recipeId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long recipeId, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetRecipeByIdQuery(recipeId), cancellationToken));
    }

    /// <summary>Updates recipe metadata (name, description, servings, times, instructions, sourceUrl).</summary>
    [HttpPut("{recipeId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long recipeId,
        [FromBody] UpdateRecipeCommand command,
        CancellationToken cancellationToken
    )
    {
        return Ok(await mediator.Send(command with { RecipeId = recipeId }, cancellationToken));
    }

    /// <summary>Replaces the entire ingredient list. Order in the array becomes the display order.</summary>
    [HttpPut("{recipeId:long}/ingredients")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReplaceIngredients(
        long recipeId,
        [FromBody] ReplaceRecipeIngredientsCommand command,
        CancellationToken cancellationToken
    )
    {
        return Ok(await mediator.Send(command with { RecipeId = recipeId }, cancellationToken));
    }

    /// <summary>Deletes the recipe. Any meal plan slot referencing it has its RecipeId nulled out (FK SET NULL).</summary>
    [HttpDelete("{recipeId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long recipeId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new DeleteRecipeCommand(recipeId), cancellationToken));

    /// <summary>
    /// AI-powered recipe parser. Returns a proposal (not saved). Frontend shows the result
    /// in an editable form; user hits Save to call POST /recipes/projects/{projectId}.
    /// </summary>
    [HttpPost("projects/{projectId:long}/parse")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ParseRecipe(
        long projectId,
        [FromBody] ParseRecipeCommand command,
        CancellationToken cancellationToken
    ) => Ok(await mediator.Send(command with { ProjectId = projectId }, cancellationToken));
}
