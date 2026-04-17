using Moq;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.ShoppingLists.AddShoppingListItem;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Tests.Features.ShoppingLists;

public class AddShoppingListItemCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IShoppingListRepository> _shoppingListRepositoryMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();

    public AddShoppingListItemCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.ShoppingLists).Returns(_shoppingListRepositoryMock.Object);
    }

    private AddShoppingListItemCommandHandler CreateHandler() => new(
        _unitOfWorkMock.Object,
        _currentUserServiceMock.Object,
        _cacheServiceMock.Object);

    private static ShoppingList BuildList(long projectId, long memberUserId)
    {
        var project = ApplicationTestUtils.CreateInstanceWithoutConstructor<Project>();
        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.Id), projectId);
        var member = ApplicationTestUtils.CreateInstanceWithoutConstructor<ProjectMember>();
        ApplicationTestUtils.SetPrivatePropertyValue(member, nameof(ProjectMember.UserId), memberUserId);
        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.ProjectMembers),
            new List<ProjectMember> { member });

        var list = ShoppingList.Create(projectId, "Groceries", memberUserId);
        ApplicationTestUtils.SetPrivatePropertyValue(list, nameof(ShoppingList.Project), project);
        return list;
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldAddItemAndPersist()
    {
        const long userId = 10L;
        const long listId = 5L;
        var list = BuildList(projectId: 42L, memberUserId: userId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _shoppingListRepositoryMock
            .Setup(r => r.GetByIdWithItemsAsync(listId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        var command = new AddShoppingListItemCommand(listId, "Milk", 2m, "L", "whole");

        var response = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Single(list.Items);
        Assert.Equal("Milk", list.Items.First().Name);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(CacheKeys.ShoppingList(list.Id), It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(CacheKeys.ProjectShoppingLists(list.ProjectId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(new AddShoppingListItemCommand(1L, "Milk", 1m, null, null), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenListNotFound_ShouldThrowNotFoundException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(1L);
        _shoppingListRepositoryMock
            .Setup(r => r.GetByIdWithItemsAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ShoppingList?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().Handle(new AddShoppingListItemCommand(99L, "Milk", 1m, null, null), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenUserNotMember_ShouldThrowAccessDeniedException()
    {
        const long listId = 5L;
        var list = BuildList(projectId: 42L, memberUserId: 999L);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(10L);

        _shoppingListRepositoryMock
            .Setup(r => r.GetByIdWithItemsAsync(listId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        await Assert.ThrowsAsync<AccessDeniedException>(() =>
            CreateHandler().Handle(new AddShoppingListItemCommand(listId, "Milk", 1m, null, null), CancellationToken.None));
    }
}
