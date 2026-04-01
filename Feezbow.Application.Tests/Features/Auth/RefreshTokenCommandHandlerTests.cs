using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Feezbow.Application.Features.Auth.RefreshToken;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Events;
using Feezbow.Domain.Exceptions;
using Feezbow.Infrastructure.Services;

namespace Feezbow.Application.Tests.Features.Auth;

public class RefreshTokenCommandHandlerTests
{
    private static Mock<UserManager<User>> CreateUserManagerMock()
    {
        return new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);
    }

    [Fact]
    public async Task Handle_WhenTokenNotFound_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new RefreshTokenCommand("nonexistent-token");

        var userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(x => x.Users)
            .Returns(new TestAsyncEnumerable<User>([]));

        var jwtTokenServiceMock = new Mock<IJwtTokenService>();

        var handler = new RefreshTokenCommandHandler(
            userManagerMock.Object,
            jwtTokenServiceMock.Object,
            NullLogger<RefreshTokenCommandHandler>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));

        jwtTokenServiceMock.Verify(
            x => x.GenerateTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTokenIsExpired_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var refreshToken = "expired-refresh-token";
        var command = new RefreshTokenCommand(refreshToken);

        var user = ApplicationTestUtils.CreateInstanceWithoutConstructor<User>();
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Id), 1L);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.IsActive), true);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.RefreshToken), refreshToken);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.RefreshTokenExpiresAt), DateTime.UtcNow.AddDays(-1));
        ApplicationTestUtils.SetPrivatePropertyValue(user, "_domainEvents", new List<DomainEvent>());

        var userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(x => x.Users)
            .Returns(new TestAsyncEnumerable<User>([user]));

        var jwtTokenServiceMock = new Mock<IJwtTokenService>();

        var handler = new RefreshTokenCommandHandler(
            userManagerMock.Object,
            jwtTokenServiceMock.Object,
            NullLogger<RefreshTokenCommandHandler>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));

        jwtTokenServiceMock.Verify(
            x => x.GenerateTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIsInactive_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        var refreshToken = "valid-but-inactive-user-token";
        var command = new RefreshTokenCommand(refreshToken);

        var user = ApplicationTestUtils.CreateInstanceWithoutConstructor<User>();
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Id), 1L);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.IsActive), false);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.RefreshToken), refreshToken);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.RefreshTokenExpiresAt), DateTime.UtcNow.AddDays(7));
        ApplicationTestUtils.SetPrivatePropertyValue(user, "_domainEvents", new List<DomainEvent>());

        var userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(x => x.Users)
            .Returns(new TestAsyncEnumerable<User>([user]));

        var jwtTokenServiceMock = new Mock<IJwtTokenService>();

        var handler = new RefreshTokenCommandHandler(
            userManagerMock.Object,
            jwtTokenServiceMock.Object,
            NullLogger<RefreshTokenCommandHandler>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleValidationException>(
            () => handler.Handle(command, CancellationToken.None));

        jwtTokenServiceMock.Verify(
            x => x.GenerateTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTokenIsValid_ShouldReturnNewTokens()
    {
        // Arrange
        var oldRefreshToken = "valid-refresh-token";
        var newRefreshToken = "new-refresh-token";
        var accessToken = "new-access-token";
        var newExpiry = DateTime.UtcNow.AddDays(7);
        var command = new RefreshTokenCommand(oldRefreshToken);

        var user = ApplicationTestUtils.CreateInstanceWithoutConstructor<User>();
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Id), 1L);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.IsActive), true);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.RefreshToken), oldRefreshToken);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.RefreshTokenExpiresAt), DateTime.UtcNow.AddDays(7));
        ApplicationTestUtils.SetPrivatePropertyValue(user, "_domainEvents", new List<DomainEvent>());

        var userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(x => x.Users)
            .Returns(new TestAsyncEnumerable<User>([user]));
        userManagerMock
            .Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        jwtTokenServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns(newRefreshToken);
        jwtTokenServiceMock
            .Setup(x => x.GetRefreshTokenExpiry())
            .Returns(newExpiry);
        jwtTokenServiceMock
            .Setup(x => x.GenerateTokenAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(accessToken);

        var handler = new RefreshTokenCommandHandler(
            userManagerMock.Object,
            jwtTokenServiceMock.Object,
            NullLogger<RefreshTokenCommandHandler>.Instance);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(accessToken, response.AccessToken);
        Assert.Equal(newRefreshToken, response.RefreshToken);

        jwtTokenServiceMock.Verify(x => x.GenerateRefreshToken(), Times.Once);
        jwtTokenServiceMock.Verify(x => x.GenerateTokenAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }
}
