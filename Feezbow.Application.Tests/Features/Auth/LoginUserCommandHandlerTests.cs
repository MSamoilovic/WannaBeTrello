using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Feezbow.Application.Features.Auth.LoginUser;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Events;
using Feezbow.Domain.Exceptions;
using Feezbow.Infrastructure.Services;

namespace Feezbow.Application.Tests.Features.Auth;

public class LoginUserCommandHandlerTests
{
    private static Mock<UserManager<User>> CreateUserManagerMock() =>
        new(Mock.Of<IUserStore<User>>(), null!, null!, null!, null!, null!, null!, null!, null!);

    private static User CreateActiveUser(string email)
    {
        var user = ApplicationTestUtils.CreateInstanceWithoutConstructor<User>();
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Id), 1L);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Email), email);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.IsActive), true);
        ApplicationTestUtils.SetPrivatePropertyValue(user, "_domainEvents", new List<DomainEvent>());
        return user;
    }

    private static LoginUserCommandHandler CreateHandler(
        Mock<UserManager<User>> userManagerMock,
        Mock<IPasswordSignInService> passwordSignInMock,
        Mock<IJwtTokenService> jwtTokenServiceMock) =>
        new(
            userManagerMock.Object,
            passwordSignInMock.Object,
            jwtTokenServiceMock.Object,
            NullLogger<LoginUserCommandHandler>.Instance);

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var usernameOrEmail = "nonexistent@example.com";
        var command = new LoginUserCommand(usernameOrEmail, "Password1!");

        var userManagerMock = CreateUserManagerMock();
        userManagerMock.Setup(x => x.FindByNameAsync(usernameOrEmail)).ReturnsAsync((User?)null);
        userManagerMock.Setup(x => x.FindByEmailAsync(usernameOrEmail)).ReturnsAsync((User?)null);

        var handler = CreateHandler(userManagerMock, new Mock<IPasswordSignInService>(), new Mock<IJwtTokenService>());

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));

        userManagerMock.Verify(x => x.FindByNameAsync(usernameOrEmail), Times.Once);
        userManagerMock.Verify(x => x.FindByEmailAsync(usernameOrEmail), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsInactive_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        var usernameOrEmail = "inactive@example.com";
        var command = new LoginUserCommand(usernameOrEmail, "Password1!");

        var user = ApplicationTestUtils.CreateInstanceWithoutConstructor<User>();
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.IsActive), false);
        ApplicationTestUtils.SetPrivatePropertyValue(user, "_domainEvents", new List<DomainEvent>());

        var userManagerMock = CreateUserManagerMock();
        userManagerMock.Setup(x => x.FindByNameAsync(usernameOrEmail)).ReturnsAsync((User?)null);
        userManagerMock.Setup(x => x.FindByEmailAsync(usernameOrEmail)).ReturnsAsync(user);

        var handler = CreateHandler(userManagerMock, new Mock<IPasswordSignInService>(), new Mock<IJwtTokenService>());

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleValidationException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenPasswordIsIncorrect_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var usernameOrEmail = "user@example.com";
        var command = new LoginUserCommand(usernameOrEmail, "WrongPassword!");

        var userManagerMock = CreateUserManagerMock();
        userManagerMock.Setup(x => x.FindByNameAsync(usernameOrEmail)).ReturnsAsync((User?)null);
        userManagerMock.Setup(x => x.FindByEmailAsync(usernameOrEmail)).ReturnsAsync(CreateActiveUser(usernameOrEmail));

        var passwordSignInMock = new Mock<IPasswordSignInService>();
        passwordSignInMock
            .Setup(x => x.CheckPasswordSignInAsync(It.IsAny<User>(), command.Password, false))
            .ReturnsAsync(SignInResult.Failed);

        var handler = CreateHandler(userManagerMock, passwordSignInMock, new Mock<IJwtTokenService>());

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenLoginSucceeds_ShouldReturnTokenAndRefreshToken()
    {
        // Arrange
        var usernameOrEmail = "user@example.com";
        var password = "Password1!";
        var command = new LoginUserCommand(usernameOrEmail, password);
        var user = CreateActiveUser(usernameOrEmail);

        var accessToken = "generated-access-token";
        var refreshToken = "generated-refresh-token";
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        var userManagerMock = CreateUserManagerMock();
        userManagerMock.Setup(x => x.FindByNameAsync(usernameOrEmail)).ReturnsAsync((User?)null);
        userManagerMock.Setup(x => x.FindByEmailAsync(usernameOrEmail)).ReturnsAsync(user);
        userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var passwordSignInMock = new Mock<IPasswordSignInService>();
        passwordSignInMock
            .Setup(x => x.CheckPasswordSignInAsync(user, password, false))
            .ReturnsAsync(SignInResult.Success);

        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        jwtTokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns(refreshToken);
        jwtTokenServiceMock.Setup(x => x.GetRefreshTokenExpiry()).Returns(refreshTokenExpiry);
        jwtTokenServiceMock.Setup(x => x.GenerateTokenAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync(accessToken);

        var handler = CreateHandler(userManagerMock, passwordSignInMock, jwtTokenServiceMock);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(accessToken, response.Token);
        Assert.Equal(refreshToken, response.RefreshToken);
        Assert.Equal(usernameOrEmail, response.Email);

        jwtTokenServiceMock.Verify(x => x.GenerateTokenAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        jwtTokenServiceMock.Verify(x => x.GenerateRefreshToken(), Times.Once);
        userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }
}
