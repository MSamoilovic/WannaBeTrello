using Moq;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.ShoppingLists.GetShoppingListById;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Tests.Features.ShoppingLists;

public class GetShoppingListByIdQueryHandlerTests
{
    private readonly Mock<IShoppingListService> _shoppingListServiceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();

    public GetShoppingListByIdQueryHandlerTests()
    {
        _cacheServiceMock.Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<GetShoppingListByIdQueryResponse>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<GetShoppingListByIdQueryResponse>>, TimeSpan?, CancellationToken>(
                (_, factory, _, _) => factory());
    }

    private GetShoppingListByIdQueryHandler CreateHandler() => new(
        _shoppingListServiceMock.Object,
        _currentUserServiceMock.Object,
        _cacheServiceMock.Object);

    private static ShoppingList BuildList(long projectId, long memberUserId, long listId)
    {
        var list = ShoppingList.Create(projectId, "Groceries", memberUserId);
        ApplicationTestUtils.SetPrivatePropertyValue(list, nameof(ShoppingList.Id), listId);
        list.AddItem("Milk", 2m, "L", null, memberUserId);
        list.AddItem("Bread", 1m, null, null, memberUserId);
        return list;
    }

    [Fact]
    public async Task Handle_WhenAuthenticatedMember_ReturnsMappedResponse()
    {
        const long userId = 10L;
        const long listId = 5L;
        var list = BuildList(projectId: 42L, memberUserId: userId, listId: listId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _shoppingListServiceMock
            .Setup(s => s.GetByIdAsync(listId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        var response = await CreateHandler().Handle(new GetShoppingListByIdQuery(listId), CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal(listId, response.Id);
        Assert.Equal("Groceries", response.Name);
        Assert.Equal(2, response.Items.Count);

        _cacheServiceMock.Verify(c => c.GetOrSetAsync(
            It.Is<string>(k => k == CacheKeys.ShoppingList(listId)),
            It.IsAny<Func<Task<GetShoppingListByIdQueryResponse>>>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(new GetShoppingListByIdQuery(1L), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenServiceThrowsNotFound_PropagatesException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(1L);

        _shoppingListServiceMock
            .Setup(s => s.GetByIdAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("ShoppingList", 99L));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().Handle(new GetShoppingListByIdQuery(99L), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenServiceThrowsAccessDenied_PropagatesException()
    {
        const long listId = 5L;
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(10L);

        _shoppingListServiceMock
            .Setup(s => s.GetByIdAsync(listId, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AccessDeniedException("Not a member"));

        await Assert.ThrowsAsync<AccessDeniedException>(() =>
            CreateHandler().Handle(new GetShoppingListByIdQuery(listId), CancellationToken.None));
    }
}
