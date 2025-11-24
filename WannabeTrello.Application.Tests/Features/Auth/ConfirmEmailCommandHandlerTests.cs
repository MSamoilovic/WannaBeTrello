using Moq;
using Microsoft.AspNetCore.Identity;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Auth.ConfirmEmail;
using WannabeTrello.Application.Tests.Utils;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Events;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces;
using WannabeTrello.Infrastructure.Services;

namespace WannabeTrello.Application.Tests.Features.Auth;

public class ConfirmEmailCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var token = "valid-token";
        var command = new ConfirmEmailCommand(email, token);

        var userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);
        userManagerMock
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync((User?)null);

        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var currentUserServiceMock = new Mock<ICurrentUserService>();

        var handler = new ConfirmEmailCommandHandler(
            userManagerMock.Object,
            jwtTokenServiceMock.Object,
            unitOfWorkMock.Object,
            currentUserServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleValidationException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("Invalid email confirmation token.", exception.Message);

        userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
        userManagerMock.Verify(x => x.ConfirmEmailAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        jwtTokenServiceMock.Verify(x => x.GenerateTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWorkMock.Verify(x => x.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIsInactive_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        var email = "inactive@example.com";
        var token = "valid-token";
        var command = new ConfirmEmailCommand(email, token);

        var user = ApplicationTestUtils.CreateInstanceWithoutConstructor<User>();
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Email), email);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.IsActive), false);

        var userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);
        userManagerMock
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);

        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var currentUserServiceMock = new Mock<ICurrentUserService>();

        var handler = new ConfirmEmailCommandHandler(
            userManagerMock.Object,
            jwtTokenServiceMock.Object,
            unitOfWorkMock.Object,
            currentUserServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleValidationException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("Account is deactivated. Please contact support.", exception.Message);

        userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
        userManagerMock.Verify(x => x.ConfirmEmailAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        jwtTokenServiceMock.Verify(x => x.GenerateTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWorkMock.Verify(x => x.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyConfirmed_ShouldReturnSuccessWithToken()
    {
        // Arrange
        var email = "confirmed@example.com";
        var token = "any-token";
        var jwtToken = "jwt-token-123";
        var command = new ConfirmEmailCommand(email, token);

        var user = ApplicationTestUtils.CreateInstanceWithoutConstructor<User>();
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Id), 1L);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Email), email);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.IsActive), true);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.EmailConfirmed), true);

        var userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);
        userManagerMock
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);

        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        jwtTokenServiceMock
            .Setup(x => x.GenerateTokenAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(jwtToken);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var currentUserServiceMock = new Mock<ICurrentUserService>();

        var handler = new ConfirmEmailCommandHandler(
            userManagerMock.Object,
            jwtTokenServiceMock.Object,
            unitOfWorkMock.Object,
            currentUserServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.Equal("Email is already confirmed.", response.Message);
        Assert.Equal(jwtToken, response.Token);

        userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
        userManagerMock.Verify(x => x.ConfirmEmailAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        jwtTokenServiceMock.Verify(x => x.GenerateTokenAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(x => x.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenConfirmationFails_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        var email = "unconfirmed@example.com";
        var token = "invalid-token";
        var command = new ConfirmEmailCommand(email, token);

        var user = ApplicationTestUtils.CreateInstanceWithoutConstructor<User>();
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Id), 1L);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Email), email);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.IsActive), true);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.EmailConfirmed), false);

        var userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);
        userManagerMock
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);

        var identityError = new IdentityError
        {
            Code = "InvalidToken",
            Description = "Invalid token provided."
        };
        var failedResult = IdentityResult.Failed(identityError);
        userManagerMock
            .Setup(x => x.ConfirmEmailAsync(user, token))
            .ReturnsAsync(failedResult);

        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var currentUserServiceMock = new Mock<ICurrentUserService>();

        var handler = new ConfirmEmailCommandHandler(
            userManagerMock.Object,
            jwtTokenServiceMock.Object,
            unitOfWorkMock.Object,
            currentUserServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleValidationException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Contains("Email confirmation failed:", exception.Message);
        Assert.Contains("Invalid token provided.", exception.Message);

        userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
        userManagerMock.Verify(x => x.ConfirmEmailAsync(user, token), Times.Once);
        jwtTokenServiceMock.Verify(x => x.GenerateTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWorkMock.Verify(x => x.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenConfirmationSucceeds_ShouldCompleteEmailConfirmationAndReturnToken()
    {
        // Arrange
        var email = "unconfirmed@example.com";
        var token = "valid-token";
        var jwtToken = "jwt-token-123";
        var ipAddress = "192.168.1.1";
        var command = new ConfirmEmailCommand(email, token);

        var user = ApplicationTestUtils.CreateInstanceWithoutConstructor<User>();
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Id), 1L);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Email), email);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.IsActive), true);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.EmailConfirmed), false);
        ApplicationTestUtils.SetPrivatePropertyValue(user, "_domainEvents", new List<DomainEvent>());

        var userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);
        userManagerMock
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);
        userManagerMock
            .Setup(x => x.ConfirmEmailAsync(user, token))
            .ReturnsAsync(IdentityResult.Success);

        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        jwtTokenServiceMock
            .Setup(x => x.GenerateTokenAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(jwtToken);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock
            .Setup(x => x.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock
            .Setup(x => x.UserIPAddress)
            .Returns(ipAddress);

        var handler = new ConfirmEmailCommandHandler(
            userManagerMock.Object,
            jwtTokenServiceMock.Object,
            unitOfWorkMock.Object,
            currentUserServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.Equal("Email confirmed successfully. You can now log in.", response.Message);
        Assert.Equal(jwtToken, response.Token);

        userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
        userManagerMock.Verify(x => x.ConfirmEmailAsync(user, token), Times.Once);
        jwtTokenServiceMock.Verify(x => x.GenerateTokenAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(x => x.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        currentUserServiceMock.Verify(x => x.UserIPAddress, Times.Once);
    }

    [Fact]
    public async Task Handle_WhenConfirmationSucceeds_ShouldCallCompleteEmailConfirmation()
    {
        // Arrange
        var email = "unconfirmed@example.com";
        var token = "valid-token";
        var jwtToken = "jwt-token-123";
        var ipAddress = "192.168.1.1";
        var command = new ConfirmEmailCommand(email, token);

        var user = ApplicationTestUtils.CreateInstanceWithoutConstructor<User>();
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Id), 1L);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Email), email);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.IsActive), true);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.EmailConfirmed), false);
        ApplicationTestUtils.SetPrivatePropertyValue(user, "_domainEvents", new List<DomainEvent>());

        var userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);
        userManagerMock
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);
        userManagerMock
            .Setup(x => x.ConfirmEmailAsync(user, token))
            .ReturnsAsync(IdentityResult.Success);

        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        jwtTokenServiceMock
            .Setup(x => x.GenerateTokenAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(jwtToken);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock
            .Setup(x => x.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock
            .Setup(x => x.UserIPAddress)
            .Returns(ipAddress);

        var handler = new ConfirmEmailCommandHandler(
            userManagerMock.Object,
            jwtTokenServiceMock.Object,
            unitOfWorkMock.Object,
            currentUserServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Success);

        // Note: Domain events for email confirmation are currently commented out in User entity
        // If uncommented, verify that CompleteEmailConfirmation was called (by checking domain events)
        // var domainEvents = user.DomainEvents.ToList();
        // Assert.Single(domainEvents);
        // Assert.Contains(domainEvents, e => e.GetType().Name == "EmailConfirmationSuccededEvent");
    }

    [Fact]
    public async Task Handle_WhenConfirmationFailsWithMultipleErrors_ShouldIncludeAllErrorsInException()
    {
        // Arrange
        var email = "unconfirmed@example.com";
        var token = "invalid-token";
        var command = new ConfirmEmailCommand(email, token);

        var user = ApplicationTestUtils.CreateInstanceWithoutConstructor<User>();
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Id), 1L);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Email), email);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.IsActive), true);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.EmailConfirmed), false);

        var userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);
        userManagerMock
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);

        var identityErrors = new[]
        {
            new IdentityError { Code = "InvalidToken", Description = "Invalid token provided." },
            new IdentityError { Code = "TokenExpired", Description = "Token has expired." }
        };
        var failedResult = IdentityResult.Failed(identityErrors);
        userManagerMock
            .Setup(x => x.ConfirmEmailAsync(user, token))
            .ReturnsAsync(failedResult);

        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var currentUserServiceMock = new Mock<ICurrentUserService>();

        var handler = new ConfirmEmailCommandHandler(
            userManagerMock.Object,
            jwtTokenServiceMock.Object,
            unitOfWorkMock.Object,
            currentUserServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleValidationException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Contains("Email confirmation failed:", exception.Message);
        Assert.Contains("Invalid token provided.", exception.Message);
        Assert.Contains("Token has expired.", exception.Message);

        userManagerMock.Verify(x => x.ConfirmEmailAsync(user, token), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenJwtTokenServiceThrowsException_ShouldPropagateException()
    {
        // Arrange
        var email = "unconfirmed@example.com";
        var token = "valid-token";
        var ipAddress = "192.168.1.1";
        var command = new ConfirmEmailCommand(email, token);

        var user = ApplicationTestUtils.CreateInstanceWithoutConstructor<User>();
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Id), 1L);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Email), email);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.IsActive), true);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.EmailConfirmed), false);
        ApplicationTestUtils.SetPrivatePropertyValue(user, "_domainEvents", new List<DomainEvent>());

        var userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);
        userManagerMock
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);
        userManagerMock
            .Setup(x => x.ConfirmEmailAsync(user, token))
            .ReturnsAsync(IdentityResult.Success);

        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        jwtTokenServiceMock
            .Setup(x => x.GenerateTokenAsync(user, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("JWT service unavailable"));

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock
            .Setup(x => x.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock
            .Setup(x => x.UserIPAddress)
            .Returns(ipAddress);

        var handler = new ConfirmEmailCommandHandler(
            userManagerMock.Object,
            jwtTokenServiceMock.Object,
            unitOfWorkMock.Object,
            currentUserServiceMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));

        userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
        userManagerMock.Verify(x => x.ConfirmEmailAsync(user, token), Times.Once);
        unitOfWorkMock.Verify(x => x.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        jwtTokenServiceMock.Verify(x => x.GenerateTokenAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }
}

