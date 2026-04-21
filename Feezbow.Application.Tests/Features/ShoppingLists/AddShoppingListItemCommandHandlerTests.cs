using Moq;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.ShoppingLists.AddShoppingListItem;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Tests.Features.ShoppingLists;

public class AddShoppingListItemCommandHandlerTests
{
    private readonly Mock<IShoppingListService> _shoppingListServiceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();

    private AddShoppingListItemCommandHandler CreateHandler() => new(
        _shoppingListServiceMock.Object,
        _currentUserServiceMock.Object,
        _cacheServiceMock.Object);

    [Fact]
    public async Task Handle_WhenValidRequest_DelegatesToServiceAndInvalidatesCache()
    {
        const long userId = 10L;
        const long listId = 5L;
        const long projectId = 42L;
        const long itemId = 77L;
        var command = new AddShoppingListItemCommand(listId, "Milk", 2m, "L", "whole");

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _shoppingListServiceMock
            .Setup(s => s.AddItemAsync(listId, command.Name, command.Quantity, command.Unit, command.Notes,
                userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((projectId, itemId));

        var response = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(itemId, response.Value);

        _cacheServiceMock.Verify(c => c.RemoveAsync(CacheKeys.ShoppingList(listId), It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(CacheKeys.ProjectShoppingLists(projectId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(new AddShoppingListItemCommand(1L, "Milk", 1m, null, null), CancellationToken.None));

        _shoppingListServiceMock.Verify(s => s.AddItemAsync(
            It.IsAny<long>(), It.IsAny<string>(), It.IsAny<decimal?>(), It.IsAny<string?>(),
            It.IsAny<string?>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenServiceThrowsNotFound_PropagatesException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(1L);

        _shoppingListServiceMock
            .Setup(s => s.AddItemAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<decimal?>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("ShoppingList", 99L));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().Handle(new AddShoppingListItemCommand(99L, "Milk", 1m, null, null), CancellationToken.None));
    }
}
