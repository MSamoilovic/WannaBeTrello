using Moq;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.ShoppingLists.CreateShoppingList;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Tests.Features.ShoppingLists;

public class CreateShoppingListCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IProjectRepository> _projectRepositoryMock = new();
    private readonly Mock<IShoppingListRepository> _shoppingListRepositoryMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();

    public CreateShoppingListCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Projects).Returns(_projectRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.ShoppingLists).Returns(_shoppingListRepositoryMock.Object);
    }

    private CreateShoppingListCommandHandler CreateHandler() => new(
        _unitOfWorkMock.Object,
        _currentUserServiceMock.Object,
        _cacheServiceMock.Object);

    private static Project BuildProjectWithMember(long projectId, long userId)
    {
        var project = ApplicationTestUtils.CreateInstanceWithoutConstructor<Project>();
        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.Id), projectId);
        var member = ApplicationTestUtils.CreateInstanceWithoutConstructor<ProjectMember>();
        ApplicationTestUtils.SetPrivatePropertyValue(member, nameof(ProjectMember.UserId), userId);
        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.ProjectMembers),
            new List<ProjectMember> { member });
        return project;
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldCreateListAndInvalidateCache()
    {
        const long userId = 10L;
        const long projectId = 42L;
        var command = new CreateShoppingListCommand(projectId, "Weekly groceries");

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _projectRepositoryMock
            .Setup(r => r.GetProjectWithMembersAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildProjectWithMember(projectId, userId));

        var response = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);

        _shoppingListRepositoryMock.Verify(r => r.AddAsync(It.IsAny<ShoppingList>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(CacheKeys.ProjectShoppingLists(projectId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);
        var command = new CreateShoppingListCommand(1L, "Groceries");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(command, CancellationToken.None));

        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ShouldThrowNotFoundException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(1L);
        _projectRepositoryMock
            .Setup(r => r.GetProjectWithMembersAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().Handle(new CreateShoppingListCommand(99L, "Groceries"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenUserNotMember_ShouldThrowAccessDeniedException()
    {
        const long projectId = 42L;
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(10L);

        _projectRepositoryMock
            .Setup(r => r.GetProjectWithMembersAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildProjectWithMember(projectId, userId: 999L));

        await Assert.ThrowsAsync<AccessDeniedException>(() =>
            CreateHandler().Handle(new CreateShoppingListCommand(projectId, "Groceries"), CancellationToken.None));
    }
}
