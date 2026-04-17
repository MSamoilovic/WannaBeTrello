using Moq;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.ShoppingLists.GetShoppingListById;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Tests.Features.ShoppingLists;

public class GetShoppingListByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IShoppingListRepository> _shoppingListRepositoryMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();

    public GetShoppingListByIdQueryHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.ShoppingLists).Returns(_shoppingListRepositoryMock.Object);

        _cacheServiceMock.Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<GetShoppingListByIdQueryResponse>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<GetShoppingListByIdQueryResponse>>, TimeSpan?, CancellationToken>((_, factory, _, _) => factory());
    }

    private GetShoppingListByIdQueryHandler CreateHandler() => new(
        _unitOfWorkMock.Object,
        _currentUserServiceMock.Object,
        _cacheServiceMock.Object);

    private static ShoppingList BuildList(long projectId, long memberUserId, long listId)
    {
        var project = ApplicationTestUtils.CreateInstanceWithoutConstructor<Project>();
        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.Id), projectId);
        var member = ApplicationTestUtils.CreateInstanceWithoutConstructor<ProjectMember>();
        ApplicationTestUtils.SetPrivatePropertyValue(member, nameof(ProjectMember.UserId), memberUserId);
        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.ProjectMembers),
            new List<ProjectMember> { member });

        var list = ShoppingList.Create(projectId, "Groceries", memberUserId);
        ApplicationTestUtils.SetPrivatePropertyValue(list, nameof(ShoppingList.Id), listId);
        ApplicationTestUtils.SetPrivatePropertyValue(list, nameof(ShoppingList.Project), project);
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

        _shoppingListRepositoryMock
            .Setup(r => r.GetByIdWithItemsAsync(listId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        var response = await CreateHandler().Handle(new GetShoppingListByIdQuery(listId), CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal(listId, response.Id);
        Assert.Equal("Groceries", response.Name);
        Assert.Equal(2, response.Items.Count);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(new GetShoppingListByIdQuery(1L), CancellationToken.None));
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
            CreateHandler().Handle(new GetShoppingListByIdQuery(99L), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenUserNotMember_ShouldThrowAccessDeniedException()
    {
        const long listId = 5L;
        var list = BuildList(projectId: 42L, memberUserId: 999L, listId: listId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(10L);

        _shoppingListRepositoryMock
            .Setup(r => r.GetByIdWithItemsAsync(listId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        await Assert.ThrowsAsync<AccessDeniedException>(() =>
            CreateHandler().Handle(new GetShoppingListByIdQuery(listId), CancellationToken.None));
    }
}
