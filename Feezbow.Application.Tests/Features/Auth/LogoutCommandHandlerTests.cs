using Microsoft.AspNetCore.Identity;
using Moq;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Auth.Logout;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Events;
using Feezbow.Domain.Interfaces;

namespace Feezbow.Application.Tests.Features.Auth;

public class LogoutCommandHandlerTests
{
    private static Mock<UserManager<User>> CreateUserManagerMock()
    {
        return new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = new LogoutCommand();

        var userManagerMock = CreateUserManagerMock();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.UserId).Returns((long?)null);

        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new LogoutCommandHandler(
            userManagerMock.Object,
            currentUserServiceMock.Object,
            unitOfWorkMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal("User not found.", response.Message);

        userManagerMock.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);
        unitOfWorkMock.Verify(x => x.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnFailureResponse()
    {
        // Arrange
        var userId = 99L;
        var command = new LogoutCommand();

        var userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((User?)null);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new LogoutCommandHandler(
            userManagerMock.Object,
            currentUserServiceMock.Object,
            unitOfWorkMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal("User not found.", response.Message);

        userManagerMock.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
        unitOfWorkMock.Verify(x => x.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenLogoutSucceeds_ShouldClearRefreshTokenAndReturnSuccess()
    {
        // Arrange
        var userId = 1L;
        var command = new LogoutCommand();

        var user = ApplicationTestUtils.CreateInstanceWithoutConstructor<User>();
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Id), userId);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Email), "test@example.com");
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.IsActive), true);
        ApplicationTestUtils.SetPrivatePropertyValue(user, "_domainEvents", new List<DomainEvent>());
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.RefreshToken), "existing-refresh-token");

        var userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock
            .Setup(x => x.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new LogoutCommandHandler(
            userManagerMock.Object,
            currentUserServiceMock.Object,
            unitOfWorkMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.Equal("Logged out successfully.", response.Message);
        Assert.Null(user.RefreshToken);

        userManagerMock.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
        unitOfWorkMock.Verify(x => x.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
