using Moq;
using Microsoft.AspNetCore.Identity;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Auth.ForgotPassword;
using WannabeTrello.Application.Tests.Utils;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Events;
using WannabeTrello.Domain.Interfaces;

namespace WannabeTrello.Application.Tests.Features.Auth;

public class ForgotPasswordCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnSuccessResponse()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var command = new ForgotPasswordCommand(email);

        var userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);
        userManagerMock
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync((User?)null);

        var emailServiceMock = new Mock<IEmailService>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var currentUserServiceMock = new Mock<ICurrentUserService>();

        var handler = new ForgotPasswordCommandHandler(
            userManagerMock.Object,
            emailServiceMock.Object,
            unitOfWorkMock.Object,
            currentUserServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.Equal("If user exists, a password link has been sent", response.Message);

        userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
        userManagerMock.Verify(x => x.GeneratePasswordResetTokenAsync(It.IsAny<User>()), Times.Never);
        emailServiceMock.Verify(x => x.SendPasswordResetEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
        unitOfWorkMock.Verify(x => x.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIsInactive_ShouldReturnSuccessResponse()
    {
        // Arrange
        var email = "inactive@example.com";
        var command = new ForgotPasswordCommand(email);

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

        var handler = new ForgotPasswordCommandHandler(
            userManagerMock.Object,
            emailServiceMock.Object,
            unitOfWorkMock.Object,
            currentUserServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.Equal("If user exists, a password link has been sent", response.Message);

        userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
        userManagerMock.Verify(x => x.GeneratePasswordResetTokenAsync(It.IsAny<User>()), Times.Never);
        emailServiceMock.Verify(x => x.SendPasswordResetEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
        unitOfWorkMock.Verify(x => x.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserExistsAndIsActive_ShouldGenerateTokenAndSendEmail()
    {
        // Arrange
        var email = "active@example.com";
        var ipAddress = "192.168.1.1";
        var resetToken = "test-reset-token";
        var command = new ForgotPasswordCommand(email);

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
            .Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync(resetToken);

        var emailServiceMock = new Mock<IEmailService>();
        emailServiceMock
            .Setup(x => x.SendPasswordResetEmailAsync(
                email,
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

        var handler = new ForgotPasswordCommandHandler(
            userManagerMock.Object,
            emailServiceMock.Object,
            unitOfWorkMock.Object,
            currentUserServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.Equal("If user exists, a password link has been sent", response.Message);

        userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
        userManagerMock.Verify(x => x.GeneratePasswordResetTokenAsync(user), Times.Once);
        unitOfWorkMock.Verify(x => x.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        currentUserServiceMock.Verify(x => x.UserIPAddress, Times.Once);

        emailServiceMock.Verify(x => x.SendPasswordResetEmailAsync(
            email,
            It.IsAny<string>(),
            It.Is<string>(url => url.Contains(Uri.EscapeDataString(email)) && url.Contains(Uri.EscapeDataString(resetToken))),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserExistsAndIsActive_ShouldCallRequestPasswordReset()
    {
        // Arrange
        var email = "active@example.com";
        var ipAddress = "192.168.1.1";
        var resetToken = "test-reset-token";
        var command = new ForgotPasswordCommand(email);

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
            .Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync(resetToken);

        var emailServiceMock = new Mock<IEmailService>();
        emailServiceMock
            .Setup(x => x.SendPasswordResetEmailAsync(
                It.IsAny<string>(),
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

        var handler = new ForgotPasswordCommandHandler(
            userManagerMock.Object,
            emailServiceMock.Object,
            unitOfWorkMock.Object,
            currentUserServiceMock.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Success);

        // Verify that RequestPasswordReset was called (by checking domain events)
        var domainEvents = user.DomainEvents.ToList();
        Assert.Single(domainEvents);
        Assert.Contains(domainEvents, e => e.GetType().Name == "PasswordResetRequestedEvent");
    }

    [Fact]
    public async Task Handle_WhenEmailServiceThrowsException_ShouldPropagateException()
    {
        // Arrange
        var email = "active@example.com";
        var ipAddress = "192.168.1.1";
        var resetToken = "test-reset-token";
        var command = new ForgotPasswordCommand(email);

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
            .Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync(resetToken);

        var emailServiceMock = new Mock<IEmailService>();
        emailServiceMock
            .Setup(x => x.SendPasswordResetEmailAsync(
                It.IsAny<string>(),
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

        var handler = new ForgotPasswordCommandHandler(
            userManagerMock.Object,
            emailServiceMock.Object,
            unitOfWorkMock.Object,
            currentUserServiceMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));

        userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
        userManagerMock.Verify(x => x.GeneratePasswordResetTokenAsync(user), Times.Once);
        unitOfWorkMock.Verify(x => x.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        emailServiceMock.Verify(x => x.SendPasswordResetEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoDisplayName_ShouldUseEmailAsDisplayName()
    {
        // Arrange
        var email = "active@example.com";
        var ipAddress = "192.168.1.1";
        var resetToken = "test-reset-token";
        var command = new ForgotPasswordCommand(email);

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
            .Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync(resetToken);

        var emailServiceMock = new Mock<IEmailService>();
        emailServiceMock
            .Setup(x => x.SendPasswordResetEmailAsync(
                email,
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

        var handler = new ForgotPasswordCommandHandler(
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
        emailServiceMock.Verify(x => x.SendPasswordResetEmailAsync(
            email,
            "testuser", // DisplayName should fallback to UserName when FirstName/LastName are null
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

