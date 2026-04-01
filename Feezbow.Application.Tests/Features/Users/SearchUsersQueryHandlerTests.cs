using Moq;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Users.SearchUsers;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Tests.Features.Users;

public class SearchUsersQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var userServiceMock = new Mock<IUserService>();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(false);

        var handler = new SearchUsersQueryHandler(userServiceMock.Object, currentUserServiceMock.Object);
        var query = new SearchUsersQuery();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(query, CancellationToken.None));

        userServiceMock.Verify(x => x.SearchUsers(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var userServiceMock = new Mock<IUserService>();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(x => x.UserId).Returns((long?)null);

        var handler = new SearchUsersQueryHandler(userServiceMock.Object, currentUserServiceMock.Object);
        var query = new SearchUsersQuery();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(query, CancellationToken.None));

        userServiceMock.Verify(x => x.SearchUsers(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSearchUsersReturnsNull_ShouldThrowException()
    {
        // Arrange
        var userServiceMock = new Mock<IUserService>();
        userServiceMock
            .Setup(x => x.SearchUsers())
            .Returns((IQueryable<User>?)null!);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(x => x.UserId).Returns(1L);

        var handler = new SearchUsersQueryHandler(userServiceMock.Object, currentUserServiceMock.Object);
        var query = new SearchUsersQuery();

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(
            () => handler.Handle(query, CancellationToken.None));

        userServiceMock.Verify(x => x.SearchUsers(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAuthenticated_ShouldReturnUsersQueryable()
    {
        // Arrange
        var userId = 1L;

        var user = ApplicationTestUtils.CreateInstanceWithoutConstructor<User>();
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Id), 10L);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.UserName), "john.doe");
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Email), "john@example.com");
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.FirstName), "John");
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.LastName), "Doe");
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.IsActive), true);

        var users = new List<User> { user }.AsQueryable();

        var userServiceMock = new Mock<IUserService>();
        userServiceMock
            .Setup(x => x.SearchUsers())
            .Returns(users);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var handler = new SearchUsersQueryHandler(userServiceMock.Object, currentUserServiceMock.Object);
        var query = new SearchUsersQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Single(resultList);
        Assert.Equal(10L, resultList[0].Id);
        Assert.Equal("john.doe", resultList[0].UserName);
        Assert.Equal("john@example.com", resultList[0].Email);

        userServiceMock.Verify(x => x.SearchUsers(), Times.Once);
    }
}
