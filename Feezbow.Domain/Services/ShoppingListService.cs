using Feezbow.Domain.Entities;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Domain.Services;

public class ShoppingListService(
    IShoppingListRepository shoppingListRepository,
    IProjectRepository projectRepository,
    IUnitOfWork unitOfWork) : IShoppingListService
{
    public async Task<ShoppingList> CreateListAsync(
        long projectId,
        string name,
        long userId,
        CancellationToken cancellationToken = default)
    {
        var project = await projectRepository.GetProjectWithMembersAsync(projectId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), projectId);

        if (!project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        var list = ShoppingList.Create(projectId, name, userId);

        await shoppingListRepository.AddAsync(list, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return list;
    }

    public async Task<ShoppingList> GetByIdAsync(
        long shoppingListId,
        long userId,
        CancellationToken cancellationToken = default)
    {
        var list = await shoppingListRepository.GetByIdWithItemsAsync(shoppingListId, cancellationToken)
            ?? throw new NotFoundException(nameof(ShoppingList), shoppingListId);

        if (!list.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        return list;
    }

    public async Task<IReadOnlyList<ShoppingList>> GetByProjectAsync(
        long projectId,
        long userId,
        bool includeArchived,
        CancellationToken cancellationToken = default)
    {
        var project = await projectRepository.GetProjectWithMembersAsync(projectId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), projectId);

        if (!project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        return await shoppingListRepository.GetByProjectAsync(projectId, includeArchived, cancellationToken);
    }

    public async Task<long> RenameListAsync(
        long shoppingListId,
        string name,
        long userId,
        CancellationToken cancellationToken = default)
    {
        var list = await shoppingListRepository.GetByIdAsync(shoppingListId, cancellationToken)
            ?? throw new NotFoundException(nameof(ShoppingList), shoppingListId);

        if (!list.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        list.Rename(name, userId);
        await unitOfWork.CompleteAsync(cancellationToken);

        return list.ProjectId;
    }

    public async Task<long> ArchiveListAsync(
        long shoppingListId,
        long userId,
        CancellationToken cancellationToken = default)
    {
        var list = await shoppingListRepository.GetByIdAsync(shoppingListId, cancellationToken)
            ?? throw new NotFoundException(nameof(ShoppingList), shoppingListId);

        if (!list.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        list.Archive(userId);
        await unitOfWork.CompleteAsync(cancellationToken);

        return list.ProjectId;
    }

    public async Task<long> DeleteListAsync(
        long shoppingListId,
        long userId,
        CancellationToken cancellationToken = default)
    {
        var list = await shoppingListRepository.GetByIdAsync(shoppingListId, cancellationToken)
            ?? throw new NotFoundException(nameof(ShoppingList), shoppingListId);

        if (!list.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        var projectId = list.ProjectId;
        shoppingListRepository.Remove(list);
        await unitOfWork.CompleteAsync(cancellationToken);

        return projectId;
    }

    public async Task<(long ProjectId, long ItemId)> AddItemAsync(
        long shoppingListId,
        string name,
        decimal? quantity,
        string? unit,
        string? notes,
        long userId,
        CancellationToken cancellationToken = default)
    {
        var list = await shoppingListRepository.GetByIdWithItemsAsync(shoppingListId, cancellationToken)
            ?? throw new NotFoundException(nameof(ShoppingList), shoppingListId);

        if (!list.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        var item = list.AddItem(name, quantity, unit, notes, userId);
        await unitOfWork.CompleteAsync(cancellationToken);

        return (list.ProjectId, item.Id);
    }

    public async Task<long> UpdateItemAsync(
        long shoppingListId,
        long itemId,
        string? name,
        decimal? quantity,
        string? unit,
        string? notes,
        long userId,
        CancellationToken cancellationToken = default)
    {
        var list = await shoppingListRepository.GetByIdWithItemsAsync(shoppingListId, cancellationToken)
            ?? throw new NotFoundException(nameof(ShoppingList), shoppingListId);

        if (!list.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        list.UpdateItem(itemId, name, quantity, unit, notes, userId);
        await unitOfWork.CompleteAsync(cancellationToken);

        return list.ProjectId;
    }

    public async Task<long> RemoveItemAsync(
        long shoppingListId,
        long itemId,
        long userId,
        CancellationToken cancellationToken = default)
    {
        var list = await shoppingListRepository.GetByIdWithItemsAsync(shoppingListId, cancellationToken)
            ?? throw new NotFoundException(nameof(ShoppingList), shoppingListId);

        if (!list.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        list.RemoveItem(itemId, userId);
        await unitOfWork.CompleteAsync(cancellationToken);

        return list.ProjectId;
    }

    public async Task<long> ToggleItemPurchasedAsync(
        long shoppingListId,
        long itemId,
        bool isPurchased,
        long userId,
        CancellationToken cancellationToken = default)
    {
        var list = await shoppingListRepository.GetByIdWithItemsAsync(shoppingListId, cancellationToken)
            ?? throw new NotFoundException(nameof(ShoppingList), shoppingListId);

        if (!list.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        if (isPurchased)
            list.MarkItemPurchased(itemId, userId);
        else
            list.MarkItemUnpurchased(itemId, userId);

        await unitOfWork.CompleteAsync(cancellationToken);

        return list.ProjectId;
    }
}
