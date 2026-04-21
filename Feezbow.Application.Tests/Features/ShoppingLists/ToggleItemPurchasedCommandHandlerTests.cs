using Moq;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.ShoppingLists.ToggleItemPurchased;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Tests.Features.ShoppingLists;

public class ToggleItemPurchasedCommandHandlerTests
{
    private readonly Mock<IShoppingListService> _shoppingListServiceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();

    private ToggleItemPurchasedCommandHandler CreateHandler() => new(
        _shoppingListServiceMock.Object,
        _currentUserServiceMock.Object,
        _cacheServiceMock.Object);

    [Fact]
    public async Task Handle_WhenValidRequest_DelegatesToServiceAndInvalidatesCache()
    {
        const long userId = 10L;
        const long listId = 5L;
        const long itemId = 100L;
        const long projectId = 42L;

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _shoppingListServiceMock
            .Setup(s => s.ToggleItemPurchasedAsync(listId, itemId, true, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(projectId);

        var response = await CreateHandler().Handle(
            new ToggleItemPurchasedCommand(listId, itemId, true), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal(itemId, response.Value);

        _shoppingListServiceMock.Verify(s => s.ToggleItemPurchasedAsync(
            listId, itemId, true, userId, It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(CacheKeys.ShoppingList(listId), It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(CacheKeys.ProjectShoppingLists(projectId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(new ToggleItemPurchasedCommand(1L, 1L, true), CancellationToken.None));

        _shoppingListServiceMock.Verify(s => s.ToggleItemPurchasedAsync(
            It.IsAny<long>(), It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<long>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenServiceThrowsAccessDenied_PropagatesException()
    {
        const long listId = 5L;
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(10L);

        _shoppingListServiceMock
            .Setup(s => s.ToggleItemPurchasedAsync(listId, It.IsAny<long>(), It.IsAny<bool>(),
                It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AccessDeniedException("Not a member"));

        await Assert.ThrowsAsync<AccessDeniedException>(() =>
            CreateHandler().Handle(new ToggleItemPurchasedCommand(listId, 100L, true), CancellationToken.None));
    }
}
