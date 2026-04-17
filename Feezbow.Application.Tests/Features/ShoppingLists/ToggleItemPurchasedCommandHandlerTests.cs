using Moq;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.ShoppingLists.ToggleItemPurchased;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Tests.Features.ShoppingLists;

public class ToggleItemPurchasedCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IShoppingListRepository> _shoppingListRepositoryMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();

    public ToggleItemPurchasedCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.ShoppingLists).Returns(_shoppingListRepositoryMock.Object);
    }

    private ToggleItemPurchasedCommandHandler CreateHandler() => new(
        _unitOfWorkMock.Object,
        _currentUserServiceMock.Object,
        _cacheServiceMock.Object);

    private static (ShoppingList list, ShoppingListItem item) BuildListWithItem(long projectId, long memberUserId, long itemId)
    {
        var project = ApplicationTestUtils.CreateInstanceWithoutConstructor<Project>();
        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.Id), projectId);
        var member = ApplicationTestUtils.CreateInstanceWithoutConstructor<ProjectMember>();
        ApplicationTestUtils.SetPrivatePropertyValue(member, nameof(ProjectMember.UserId), memberUserId);
        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.ProjectMembers),
            new List<ProjectMember> { member });

        var list = ShoppingList.Create(projectId, "Groceries", memberUserId);
        ApplicationTestUtils.SetPrivatePropertyValue(list, nameof(ShoppingList.Project), project);

        var item = list.AddItem("Milk", 1m, null, null, memberUserId);
        ApplicationTestUtils.SetPrivatePropertyValue(item, nameof(ShoppingListItem.Id), itemId);
        return (list, item);
    }

    [Fact]
    public async Task Handle_WhenMarkingPurchased_ShouldSetFlagAndPersist()
    {
        const long userId = 10L;
        const long listId = 5L;
        const long itemId = 100L;
        var (list, item) = BuildListWithItem(projectId: 42L, memberUserId: userId, itemId: itemId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _shoppingListRepositoryMock
            .Setup(r => r.GetByIdWithItemsAsync(listId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        var response = await CreateHandler().Handle(
            new ToggleItemPurchasedCommand(listId, itemId, true), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.True(item.IsPurchased);
        Assert.Equal(userId, item.PurchasedBy);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUnmarking_ShouldClearFlag()
    {
        const long userId = 10L;
        const long listId = 5L;
        const long itemId = 100L;
        var (list, item) = BuildListWithItem(projectId: 42L, memberUserId: userId, itemId: itemId);
        list.MarkItemPurchased(itemId, userId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _shoppingListRepositoryMock
            .Setup(r => r.GetByIdWithItemsAsync(listId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        await CreateHandler().Handle(
            new ToggleItemPurchasedCommand(listId, itemId, false), CancellationToken.None);

        Assert.False(item.IsPurchased);
        Assert.Null(item.PurchasedBy);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(new ToggleItemPurchasedCommand(1L, 1L, true), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenUserNotMember_ShouldThrowAccessDeniedException()
    {
        const long listId = 5L;
        var (list, _) = BuildListWithItem(projectId: 42L, memberUserId: 999L, itemId: 100L);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(10L);

        _shoppingListRepositoryMock
            .Setup(r => r.GetByIdWithItemsAsync(listId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        await Assert.ThrowsAsync<AccessDeniedException>(() =>
            CreateHandler().Handle(new ToggleItemPurchasedCommand(listId, 100L, true), CancellationToken.None));
    }
}
