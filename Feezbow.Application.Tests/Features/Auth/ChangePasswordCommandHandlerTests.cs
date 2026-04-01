using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Auth.ChangePassword;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Events;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Tests.Features.Auth;

public class ChangePasswordCommandHandlerTests
{
    private static Mock<UserManager<User>> CreateUserManagerMock()
    {
        return new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new ChangePasswordCommand("OldPass1!", "NewPass1!", "NewPass1!");

        var userManagerMock = CreateUserManagerMock();
        var userServiceMock = new Mock<IUserService>();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(false);

        var handler = new ChangePasswordCommandHandler(
            userManagerMock.Object,
            userServiceMock.Object,
            currentUserServiceMock.Object,
            NullLogger<ChangePasswordCommandHandler>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));

        userServiceMock.Verify(x => x.GetUserProfileAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new ChangePasswordCommand("OldPass1!", "NewPass1!", "NewPass1!");

        var userManagerMock = CreateUserManagerMock();
        var userServiceMock = new Mock<IUserService>();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(x => x.UserId).Returns((long?)null);

        var handler = new ChangePasswordCommandHandler(
            userManagerMock.Object,
            userServiceMock.Object,
            currentUserServiceMock.Object,
            NullLogger<ChangePasswordCommandHandler>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));

        userServiceMock.Verify(x => x.GetUserProfileAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = 42L;
        var command = new ChangePasswordCommand("OldPass1!", "NewPass1!", "NewPass1!");

        var userManagerMock = CreateUserManagerMock();
        var userServiceMock = new Mock<IUserService>();
        userServiceMock
            .Setup(x => x.GetUserProfileAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var handler = new ChangePasswordCommandHandler(
            userManagerMock.Object,
            userServiceMock.Object,
            currentUserServiceMock.Object,
            NullLogger<ChangePasswordCommandHandler>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None));

        userServiceMock.Verify(x => x.GetUserProfileAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        userManagerMock.Verify(x => x.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenOldPasswordIsIncorrect_ShouldReturnFailResult()
    {
        // Arrange
        var userId = 1L;
        var oldPassword = "WrongOldPass1!";
        var command = new ChangePasswordCommand(oldPassword, "NewPass1!", "NewPass1!");

        var user = ApplicationTestUtils.CreateInstanceWithoutConstructor<User>();
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Id), userId);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Email), "test@example.com");
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.IsActive), true);
        ApplicationTestUtils.SetPrivatePropertyValue(user, "_domainEvents", new List<DomainEvent>());

        var userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, oldPassword))
            .ReturnsAsync(false);

        var userServiceMock = new Mock<IUserService>();
        userServiceMock
            .Setup(x => x.GetUserProfileAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var handler = new ChangePasswordCommandHandler(
            userManagerMock.Object,
            userServiceMock.Object,
            currentUserServiceMock.Object,
            NullLogger<ChangePasswordCommandHandler>.Instance);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Result.IsSuccess);
        Assert.Equal("Old Password is incorrect", response.Result.Message);

        userManagerMock.Verify(x => x.CheckPasswordAsync(user, oldPassword), Times.Once);
        userManagerMock.Verify(x => x.ChangePasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPasswordChangedSuccessfully_ShouldReturnSuccessResult()
    {
        // Arrange
        var userId = 1L;
        var oldPassword = "OldPass1!";
        var newPassword = "NewPass1!";
        var command = new ChangePasswordCommand(oldPassword, newPassword, newPassword);

        var user = ApplicationTestUtils.CreateInstanceWithoutConstructor<User>();
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Id), userId);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Email), "test@example.com");
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.IsActive), true);
        ApplicationTestUtils.SetPrivatePropertyValue(user, "_domainEvents", new List<DomainEvent>());

        var userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, oldPassword))
            .ReturnsAsync(true);
        userManagerMock
            .Setup(x => x.ChangePasswordAsync(user, oldPassword, newPassword))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var userServiceMock = new Mock<IUserService>();
        userServiceMock
            .Setup(x => x.GetUserProfileAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var handler = new ChangePasswordCommandHandler(
            userManagerMock.Object,
            userServiceMock.Object,
            currentUserServiceMock.Object,
            NullLogger<ChangePasswordCommandHandler>.Instance);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);
        Assert.Equal("Password updated successfully", response.Result.Message);
        Assert.Equal(userId, response.Result.Value);

        userManagerMock.Verify(x => x.CheckPasswordAsync(user, oldPassword), Times.Once);
        userManagerMock.Verify(x => x.ChangePasswordAsync(user, oldPassword, newPassword), Times.Once);
        userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }
}
