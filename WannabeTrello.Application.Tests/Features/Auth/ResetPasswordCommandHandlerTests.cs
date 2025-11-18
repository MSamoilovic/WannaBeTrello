using Moq;
using Microsoft.AspNetCore.Identity;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Auth.ResetPassword;
using WannabeTrello.Application.Tests.Utils;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Events;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces;

namespace WannabeTrello.Application.Tests.Features.Auth;

public class ResetPasswordCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var token = "valid-token";
        var newPassword = "NewPassword123!";
        var command = new ResetPasswordCommand(email, token, newPassword, newPassword);

        var userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);
        userManagerMock
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync((User?)null);

        var emailServiceMock = new Mock<IEmailService>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var currentUserServiceMock = new Mock<ICurrentUserService>();

        var handler = new ResetPasswordCommandHandler(
            userManagerMock.Object,
            emailServiceMock.Object,
            unitOfWorkMock.Object,
            currentUserServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleValidationException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("Invalid password reset token.", exception.Message);

        userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
        userManagerMock.Verify(x => x.ResetPasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        emailServiceMock.Verify(x => x.SendPasswordResetConfirmationEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
        unitOfWorkMock.Verify(x => x.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIsInactive_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        var email = "inactive@example.com";
        var token = "valid-token";
        var newPassword = "NewPassword123!";
        var command = new ResetPasswordCommand(email, token, newPassword, newPassword);

        var user = ApplicationTestUtils.CreateInstanceWithoutConstructor<User>();
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Email), email);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.IsActive), false);

        var userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);
        userManagerMock
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);

        var emailServiceMock = new Mock<IEmailService>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var currentUserServiceMock = new Mock<ICurrentUserService>();

        var handler = new ResetPasswordCommandHandler(
            userManagerMock.Object,
            emailServiceMock.Object,
            unitOfWorkMock.Object,
            currentUserServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleValidationException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Equal("Account is deactivated. Please contact support.", exception.Message);

        userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
        userManagerMock.Verify(x => x.ResetPasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        emailServiceMock.Verify(x => x.SendPasswordResetConfirmationEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
        unitOfWorkMock.Verify(x => x.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenResetPasswordFails_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        var email = "active@example.com";
        var token = "invalid-token";
        var newPassword = "NewPassword123!";
        var command = new ResetPasswordCommand(email, token, newPassword, newPassword);

        var user = ApplicationTestUtils.CreateInstanceWithoutConstructor<User>();
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Id), 1L);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Email), email);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.IsActive), true);
        ApplicationTestUtils.SetPrivatePropertyValue(user, "_domainEvents", new List<DomainEvent>());

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
            .Setup(x => x.ResetPasswordAsync(user, token, newPassword))
            .ReturnsAsync(failedResult);

        var emailServiceMock = new Mock<IEmailService>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var currentUserServiceMock = new Mock<ICurrentUserService>();

        var handler = new ResetPasswordCommandHandler(
            userManagerMock.Object,
            emailServiceMock.Object,
            unitOfWorkMock.Object,
            currentUserServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleValidationException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Contains("Password reset failed:", exception.Message);
        Assert.Contains("Invalid token provided.", exception.Message);

        userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
        userManagerMock.Verify(x => x.ResetPasswordAsync(user, token, newPassword), Times.Once);
        emailServiceMock.Verify(x => x.SendPasswordResetConfirmationEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
        unitOfWorkMock.Verify(x => x.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenResetPasswordSucceeds_ShouldCompletePasswordResetAndSendEmail()
    {
        // Arrange
        var email = "active@example.com";
        var token = "valid-token";
        var newPassword = "NewPassword123!";
        var ipAddress = "192.168.1.1";
        var command = new ResetPasswordCommand(email, token, newPassword, newPassword);

        var user = ApplicationTestUtils.CreateInstanceWithoutConstructor<User>();
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Id), 1L);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Email), email);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.IsActive), true);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.FirstName), "John");
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.LastName), "Doe");
        ApplicationTestUtils.SetPrivatePropertyValue(user, "_domainEvents", new List<DomainEvent>());

        var userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);
        userManagerMock
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);
        userManagerMock
            .Setup(x => x.ResetPasswordAsync(user, token, newPassword))
            .ReturnsAsync(IdentityResult.Success);

        var emailServiceMock = new Mock<IEmailService>();
        emailServiceMock
            .Setup(x => x.SendPasswordResetConfirmationEmailAsync(
                email,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock
            .Setup(x => x.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock
            .Setup(x => x.UserIPAddress)
            .Returns(ipAddress);

        var handler = new ResetPasswordCommandHandler(
            userManagerMock.Object,
            emailServiceMock.Object,
            unitOfWorkMock.Object,
            currentUserServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.Equal("Password has been reset successfully. You can now log in with your new password.", response.Message);

        userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
        userManagerMock.Verify(x => x.ResetPasswordAsync(user, token, newPassword), Times.Once);
        unitOfWorkMock.Verify(x => x.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        currentUserServiceMock.Verify(x => x.UserIPAddress, Times.Once);

        emailServiceMock.Verify(x => x.SendPasswordResetConfirmationEmailAsync(
            email,
            "John Doe", // DisplayName should be "John Doe"
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenResetPasswordSucceeds_ShouldCallCompletePasswordReset()
    {
        // Arrange
        var email = "active@example.com";
        var token = "valid-token";
        var newPassword = "NewPassword123!";
        var ipAddress = "192.168.1.1";
        var command = new ResetPasswordCommand(email, token, newPassword, newPassword);

        var user = ApplicationTestUtils.CreateInstanceWithoutConstructor<User>();
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Id), 1L);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Email), email);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.IsActive), true);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.FirstName), "John");
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.LastName), "Doe");
        ApplicationTestUtils.SetPrivatePropertyValue(user, "_domainEvents", new List<DomainEvent>());

        var userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);
        userManagerMock
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);
        userManagerMock
            .Setup(x => x.ResetPasswordAsync(user, token, newPassword))
            .ReturnsAsync(IdentityResult.Success);

        var emailServiceMock = new Mock<IEmailService>();
        emailServiceMock
            .Setup(x => x.SendPasswordResetConfirmationEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock
            .Setup(x => x.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock
            .Setup(x => x.UserIPAddress)
            .Returns(ipAddress);

        var handler = new ResetPasswordCommandHandler(
            userManagerMock.Object,
            emailServiceMock.Object,
            unitOfWorkMock.Object,
            currentUserServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Success);

        // Verify that CompletePasswordReset was called (by checking domain events)
        var domainEvents = user.DomainEvents.ToList();
        Assert.Single(domainEvents);
        Assert.Contains(domainEvents, e => e.GetType().Name == "PasswordResetCompletedEvent");
    }

    [Fact]
    public async Task Handle_WhenResetPasswordSucceedsWithMultipleErrors_ShouldIncludeAllErrorsInException()
    {
        // Arrange
        var email = "active@example.com";
        var token = "invalid-token";
        var newPassword = "weak";
        var command = new ResetPasswordCommand(email, token, newPassword, newPassword);

        var user = ApplicationTestUtils.CreateInstanceWithoutConstructor<User>();
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Id), 1L);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Email), email);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.IsActive), true);
        ApplicationTestUtils.SetPrivatePropertyValue(user, "_domainEvents", new List<DomainEvent>());

        var userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);
        userManagerMock
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);

        var identityErrors = new[]
        {
            new IdentityError { Code = "InvalidToken", Description = "Invalid token provided." },
            new IdentityError { Code = "PasswordTooShort", Description = "Password is too short." }
        };
        var failedResult = IdentityResult.Failed(identityErrors);
        userManagerMock
            .Setup(x => x.ResetPasswordAsync(user, token, newPassword))
            .ReturnsAsync(failedResult);

        var emailServiceMock = new Mock<IEmailService>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var currentUserServiceMock = new Mock<ICurrentUserService>();

        var handler = new ResetPasswordCommandHandler(
            userManagerMock.Object,
            emailServiceMock.Object,
            unitOfWorkMock.Object,
            currentUserServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleValidationException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Contains("Password reset failed:", exception.Message);
        Assert.Contains("Invalid token provided.", exception.Message);
        Assert.Contains("Password is too short.", exception.Message);

        userManagerMock.Verify(x => x.ResetPasswordAsync(user, token, newPassword), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEmailServiceThrowsException_ShouldPropagateException()
    {
        // Arrange
        var email = "active@example.com";
        var token = "valid-token";
        var newPassword = "NewPassword123!";
        var ipAddress = "192.168.1.1";
        var command = new ResetPasswordCommand(email, token, newPassword, newPassword);

        var user = ApplicationTestUtils.CreateInstanceWithoutConstructor<User>();
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Id), 1L);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Email), email);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.IsActive), true);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.FirstName), "John");
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.LastName), "Doe");
        ApplicationTestUtils.SetPrivatePropertyValue(user, "_domainEvents", new List<DomainEvent>());

        var userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);
        userManagerMock
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);
        userManagerMock
            .Setup(x => x.ResetPasswordAsync(user, token, newPassword))
            .ReturnsAsync(IdentityResult.Success);

        var emailServiceMock = new Mock<IEmailService>();
        emailServiceMock
            .Setup(x => x.SendPasswordResetConfirmationEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Email service unavailable"));

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock
            .Setup(x => x.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock
            .Setup(x => x.UserIPAddress)
            .Returns(ipAddress);

        var handler = new ResetPasswordCommandHandler(
            userManagerMock.Object,
            emailServiceMock.Object,
            unitOfWorkMock.Object,
            currentUserServiceMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));

        userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
        userManagerMock.Verify(x => x.ResetPasswordAsync(user, token, newPassword), Times.Once);
        unitOfWorkMock.Verify(x => x.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        emailServiceMock.Verify(x => x.SendPasswordResetConfirmationEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoDisplayName_ShouldUseEmailAsDisplayName()
    {
        // Arrange
        var email = "active@example.com";
        var token = "valid-token";
        var newPassword = "NewPassword123!";
        var ipAddress = "192.168.1.1";
        var command = new ResetPasswordCommand(email, token, newPassword, newPassword);

        var user = ApplicationTestUtils.CreateInstanceWithoutConstructor<User>();
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Id), 1L);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Email), email);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.UserName), "testuser");
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.IsActive), true);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.FirstName), null);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.LastName), null);
        ApplicationTestUtils.SetPrivatePropertyValue(user, "_domainEvents", new List<DomainEvent>());

        var userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);
        userManagerMock
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);
        userManagerMock
            .Setup(x => x.ResetPasswordAsync(user, token, newPassword))
            .ReturnsAsync(IdentityResult.Success);

        var emailServiceMock = new Mock<IEmailService>();
        emailServiceMock
            .Setup(x => x.SendPasswordResetConfirmationEmailAsync(
                email,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock
            .Setup(x => x.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock
            .Setup(x => x.UserIPAddress)
            .Returns(ipAddress);

        var handler = new ResetPasswordCommandHandler(
            userManagerMock.Object,
            emailServiceMock.Object,
            unitOfWorkMock.Object,
            currentUserServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Success);

        // Verify that email was sent with user's display name (should be username in this case)
        emailServiceMock.Verify(x => x.SendPasswordResetConfirmationEmailAsync(
            email,
            "testuser", // DisplayName should fallback to UserName when FirstName/LastName are null
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

