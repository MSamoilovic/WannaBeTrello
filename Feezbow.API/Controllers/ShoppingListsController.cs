using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Feezbow.Application.Features.ShoppingLists.AddShoppingListItem;
using Feezbow.Application.Features.ShoppingLists.ArchiveShoppingList;
using Feezbow.Application.Features.ShoppingLists.CreateShoppingList;
using Feezbow.Application.Features.ShoppingLists.DeleteShoppingList;
using Feezbow.Application.Features.ShoppingLists.GetShoppingListById;
using Feezbow.Application.Features.ShoppingLists.GetShoppingListsByProject;
using Feezbow.Application.Features.ShoppingLists.RemoveShoppingListItem;
using Feezbow.Application.Features.ShoppingLists.RenameShoppingList;
using Feezbow.Application.Features.ShoppingLists.ToggleItemPurchased;
using Feezbow.Application.Features.ShoppingLists.UpdateShoppingListItem;

namespace Feezbow.Controllers;

[Authorize(Policy = "EmailConfirmed")]
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/projects/{projectId:long}/shopping-lists")]
public class ShoppingListsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLists(long projectId, [FromQuery] bool includeArchived, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetShoppingListsByProjectQuery(projectId, includeArchived), cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateList(long projectId, [FromBody] CreateShoppingListCommand command, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(command with { ProjectId = projectId }, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{listId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetList(long projectId, long listId, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetShoppingListByIdQuery(listId), cancellationToken);
        return Ok(response);
    }

    [HttpPut("{listId:long}/rename")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RenameList(long projectId, long listId, [FromBody] RenameShoppingListCommand command, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(command with { ShoppingListId = listId }, cancellationToken);
        return Ok(response);
    }

    [HttpPost("{listId:long}/archive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ArchiveList(long projectId, long listId, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new ArchiveShoppingListCommand(listId), cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{listId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteList(long projectId, long listId, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new DeleteShoppingListCommand(listId), cancellationToken);
        return Ok(response);
    }

    [HttpPost("{listId:long}/items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddItem(long projectId, long listId, [FromBody] AddShoppingListItemCommand command, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(command with { ShoppingListId = listId }, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{listId:long}/items/{itemId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateItem(long projectId, long listId, long itemId, [FromBody] UpdateShoppingListItemCommand command, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(command with { ShoppingListId = listId, ItemId = itemId }, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{listId:long}/items/{itemId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItem(long projectId, long listId, long itemId, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new RemoveShoppingListItemCommand(listId, itemId), cancellationToken);
        return Ok(response);
    }

    [HttpPatch("{listId:long}/items/{itemId:long}/purchased")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleItemPurchased(long projectId, long listId, long itemId, [FromBody] ToggleItemPurchasedCommand command, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(command with { ShoppingListId = listId, ItemId = itemId }, cancellationToken);
        return Ok(response);
    }
}
