using Moq;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.ShoppingLists.CreateShoppingList;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Tests.Features.ShoppingLists;

public class CreateShoppingListCommandHandlerTests
{
    private readonly Mock<IShoppingListService> _shoppingListServiceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();

    private CreateShoppingListCommandHandler CreateHandler() => new(
        _shoppingListServiceMock.Object,
        _currentUserServiceMock.Object,
        _cacheServiceMock.Object);

    private static ShoppingList BuildList(long id, long projectId, long userId)
    {
        var list = ShoppingList.Create(projectId, "Groceries", userId);
        ApplicationTestUtils.SetPrivatePropertyValue(list, nameof(ShoppingList.Id), id);
        return list;
    }

    [Fact]
    public async Task Handle_WhenValidRequest_DelegatesToServiceAndInvalidatesCache()
    {
        const long userId = 10L;
        const long projectId = 42L;
        const long listId = 7L;
        var command = new CreateShoppingListCommand(projectId, "Weekly groceries");

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _shoppingListServiceMock
            .Setup(s => s.CreateListAsync(projectId, command.Name, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildList(listId, projectId, userId));

        var response = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);
        Assert.Equal(listId, response.Result.Value);
        _cacheServiceMock.Verify(c => c.RemoveAsync(CacheKeys.ProjectShoppingLists(projectId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);
        var command = new CreateShoppingListCommand(1L, "Groceries");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(command, CancellationToken.None));

        _shoppingListServiceMock.Verify(s => s.CreateListAsync(
            It.IsAny<long>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenServiceThrowsNotFound_PropagatesException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(1L);

        _shoppingListServiceMock
            .Setup(s => s.CreateListAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Project", 99L));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().Handle(new CreateShoppingListCommand(99L, "Groceries"), CancellationToken.None));
    }
}
