using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Auth.RegisterUser;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Events;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Tests.Features.Auth;

public class RegisterUserCommandHandlerTests
{
    private static Mock<UserManager<User>> CreateUserManagerMock()
    {
        return new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static User CreateValidUser(string email = "test@example.com", string userName = "testuser")
    {
        var user = ApplicationTestUtils.CreateInstanceWithoutConstructor<User>();
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Id), 1L);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.Email), email);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.UserName), userName);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.IsActive), true);
        ApplicationTestUtils.SetPrivatePropertyValue(user, nameof(User.EmailConfirmed), false);
        ApplicationTestUtils.SetPrivatePropertyValue(user, "_domainEvents", new List<DomainEvent>());
        return user;
    }

    [Fact]
    public async Task Handle_WhenUserCreationFails_ShouldThrowValidationException()
    {
        // Arrange
        var command = new RegisterUserCommand("testuser", "test@example.com", "Password1!");

        var user = CreateValidUser();

        var userServiceMock = new Mock<IUserService>();
        userServiceMock
            .Setup(x => x.CreateUserForAuth(command.UserName, command.Email, null, null, null, null, null))
            .Returns(user);

        var identityErrors = new[]
        {
            new IdentityError { Code = "DuplicateEmail", Description = "Email is already taken." }
        };
        var failedResult = IdentityResult.Failed(identityErrors);

        var userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(x => x.CreateAsync(user, command.Password))
            .ReturnsAsync(failedResult);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var emailServiceMock = new Mock<IEmailService>();

        var handler = new RegisterUserCommandHandler(
            userServiceMock.Object,
            userManagerMock.Object,
            currentUserServiceMock.Object,
            unitOfWorkMock.Object,
            emailServiceMock.Object,
            NullLogger<RegisterUserCommandHandler>.Instance,
            Mock.Of<IConfiguration>());

        // Act & Assert
        await Assert.ThrowsAsync<Feezbow.Application.Common.Exceptions.ValidationException>(
            () => handler.Handle(command, CancellationToken.None));

        userManagerMock.Verify(x => x.CreateAsync(user, command.Password), Times.Once);
        userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        emailServiceMock.Verify(
            x => x.SendEmailConfirmationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRegistrationSucceeds_ShouldReturnEmailAndConfirmationStatus()
    {
        // Arrange
        var email = "newuser@example.com";
        var userName = "newuser";
        var command = new RegisterUserCommand(userName, email, "Password1!");

        var user = CreateValidUser(email, userName);

        var userServiceMock = new Mock<IUserService>();
        userServiceMock
            .Setup(x => x.CreateUserForAuth(command.UserName, command.Email, null, null, null, null, null))
            .Returns(user);

        var userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(x => x.CreateAsync(user, command.Password))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(x => x.AddToRoleAsync(user, "User"))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
            .ReturnsAsync("email-confirmation-token");

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.UserIPAddress).Returns("127.0.0.1");

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock
            .Setup(x => x.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var emailServiceMock = new Mock<IEmailService>();
        emailServiceMock
            .Setup(x => x.SendEmailConfirmationEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new RegisterUserCommandHandler(
            userServiceMock.Object,
            userManagerMock.Object,
            currentUserServiceMock.Object,
            unitOfWorkMock.Object,
            emailServiceMock.Object,
            NullLogger<RegisterUserCommandHandler>.Instance,
            Mock.Of<IConfiguration>());

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(email, response.Email);
        Assert.False(response.EmailConfirmed);
    }

    [Fact]
    public async Task Handle_WhenRegistrationSucceeds_ShouldSendConfirmationEmail()
    {
        // Arrange
        var email = "newuser@example.com";
        var userName = "newuser";
        var command = new RegisterUserCommand(userName, email, "Password1!");

        var user = CreateValidUser(email, userName);

        var userServiceMock = new Mock<IUserService>();
        userServiceMock
            .Setup(x => x.CreateUserForAuth(command.UserName, command.Email, null, null, null, null, null))
            .Returns(user);

        var userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(x => x.CreateAsync(user, command.Password))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(x => x.AddToRoleAsync(user, "User"))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
            .ReturnsAsync("email-confirmation-token");

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.UserIPAddress).Returns("127.0.0.1");

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock
            .Setup(x => x.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var emailServiceMock = new Mock<IEmailService>();
        emailServiceMock
            .Setup(x => x.SendEmailConfirmationEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new RegisterUserCommandHandler(
            userServiceMock.Object,
            userManagerMock.Object,
            currentUserServiceMock.Object,
            unitOfWorkMock.Object,
            emailServiceMock.Object,
            NullLogger<RegisterUserCommandHandler>.Instance,
            Mock.Of<IConfiguration>());

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        emailServiceMock.Verify(
            x => x.SendEmailConfirmationEmailAsync(
                email,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        unitOfWorkMock.Verify(x => x.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
