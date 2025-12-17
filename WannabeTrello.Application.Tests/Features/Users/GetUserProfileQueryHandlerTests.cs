using System.Runtime.Serialization;
using Moq;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Users.GetUserProfile;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.Users;

public class GetUserProfileQueryHandlerTests
{
    private static T CreateInstanceWithoutConstructor<T>() where T : class
    {
        return (T)FormatterServices.GetUninitializedObject(typeof(T));
    }
    
    private static void SetPrivatePropertyValue<T>(T obj, string propertyName, object value) where T : class
    {
        var property = typeof(T).GetProperty(propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var currentType = typeof(T).BaseType;
        while (currentType != null && property == null)
        {
            property = currentType.GetProperty(propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            currentType = currentType.BaseType;
        }
        property?.SetValue(obj, value, null);
    }
    
    private static void SetPrivateFieldValue<T>(T obj, string fieldName, object value)
    {
        var field = typeof(T).GetField(
            fieldName,
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance
        );

        if (field == null)
        {
            throw new InvalidOperationException($"Private field '{fieldName}' not found on type '{typeof(T).Name}'.");
        }
        
        field.SetValue(obj, value);
    }
    
    [Fact]
    public async Task Handle_WhenUserIsAuthenticatedAndRequestedUserExists_ShouldReturnPublicProfile()
    {
        // Arrange
        const long currentUserId = 123L;
        const long requestedUserId = 456L;
        var query = new GetUserProfileQuery(requestedUserId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var cachingServiceMock = new Mock<ICacheService>();
        cachingServiceMock.Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<User?>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<User?>>, TimeSpan?, CancellationToken>((_, factory, _, _) => factory());

        var userServiceMock = new Mock<IUserService>();
        var userFromService = CreateInstanceWithoutConstructor<User>();
        SetPrivatePropertyValue(userFromService, nameof(User.Id), requestedUserId);
        SetPrivatePropertyValue(userFromService, nameof(User.UserName), "otheruser");
        SetPrivatePropertyValue(userFromService, nameof(User.Email), "other@example.com");
        SetPrivatePropertyValue(userFromService, nameof(User.FirstName), "Other");
        SetPrivatePropertyValue(userFromService, nameof(User.LastName), "User");
        SetPrivatePropertyValue(userFromService, nameof(User.Bio), "Other user bio");
        SetPrivatePropertyValue(userFromService, nameof(User.CreatedAt), DateTime.UtcNow);
        
        // Initialize collections for counts
        SetPrivateFieldValue(userFromService, "_assignedTasks", new List<BoardTask>());
        SetPrivateFieldValue(userFromService, "_projectMemberships", new List<ProjectMember>());

        userServiceMock
            .Setup(s => s.GetUserProfileAsync(requestedUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userFromService);

        var handler = new GetUserProfileQueryHandler(userServiceMock.Object, currentUserServiceMock.Object, cachingServiceMock.Object);

        // Act
        var response = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(requestedUserId, response.Id);
        Assert.Equal("otheruser", response.UserName);
        Assert.Equal("Other", response.FirstName);
        Assert.Equal("User", response.LastName);
        Assert.Equal("Other user bio", response.Bio);
        
        // Verify that private fields are NOT exposed
        Assert.Null(response.GetType().GetProperty("Email"));
        Assert.Null(response.GetType().GetProperty("PhoneNumber"));

        userServiceMock.Verify(s => s.GetUserProfileAsync(requestedUserId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        const long requestedUserId = 456L;
        var query = new GetUserProfileQuery(requestedUserId);
        
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var userServiceMock = new Mock<IUserService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new GetUserProfileQueryHandler(userServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            handler.Handle(query, CancellationToken.None));
        
        Assert.Equal("User is not authenticated", exception.Message);
        userServiceMock.Verify(s => s.GetUserProfileAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        const long requestedUserId = 456L;
        var query = new GetUserProfileQuery(requestedUserId);
        
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        var userServiceMock = new Mock<IUserService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new GetUserProfileQueryHandler(userServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            handler.Handle(query, CancellationToken.None));
        
        Assert.Equal("User is not authenticated", exception.Message);
        userServiceMock.Verify(s => s.GetUserProfileAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WhenRequestedUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        const long currentUserId = 123L;
        const long requestedUserId = 999L;
        var query = new GetUserProfileQuery(requestedUserId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(currentUserId);

        var userServiceMock = new Mock<IUserService>();
        userServiceMock
            .Setup(s => s.GetUserProfileAsync(requestedUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var cacheServiceMock = new Mock<ICacheService>();
        cacheServiceMock.Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<User?>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<User?>>, TimeSpan?, CancellationToken>((_, factory, _, _) => factory());

        var handler = new GetUserProfileQueryHandler(userServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(query, CancellationToken.None));
    }
    
    [Fact]
    public async Task Handle_WhenRequestingOwnProfile_ShouldReturnPublicProfileVersion()
    {
        // Arrange
        const long userId = 123L;
        var query = new GetUserProfileQuery(userId);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var userServiceMock = new Mock<IUserService>();
        var userFromService = CreateInstanceWithoutConstructor<User>();
        SetPrivatePropertyValue(userFromService, nameof(User.Id), userId);
        SetPrivatePropertyValue(userFromService, nameof(User.UserName), "testuser");
        SetPrivatePropertyValue(userFromService, nameof(User.Email), "test@example.com");
        SetPrivatePropertyValue(userFromService, nameof(User.FirstName), "Test");
        SetPrivatePropertyValue(userFromService, nameof(User.LastName), "User");
        SetPrivatePropertyValue(userFromService, nameof(User.Bio), "Test bio");
        SetPrivatePropertyValue(userFromService, nameof(User.CreatedAt), DateTime.UtcNow);
        
        SetPrivateFieldValue(userFromService, "_assignedTasks", new List<BoardTask>());
        SetPrivateFieldValue(userFromService, "_projectMemberships", new List<ProjectMember>());

        userServiceMock
            .Setup(s => s.GetUserProfileAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userFromService);

        var cacheServiceMock = new Mock<ICacheService>();
        cacheServiceMock.Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<User?>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<User?>>, TimeSpan?, CancellationToken>((_, factory, _, _) => factory());

        var handler = new GetUserProfileQueryHandler(userServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var response = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(userId, response.Id);
        Assert.Equal("testuser", response.UserName);
        
        // Even when requesting own profile via this endpoint, it should return public version
        Assert.Null(response.GetType().GetProperty("Email"));
        Assert.Null(response.GetType().GetProperty("PhoneNumber"));
    }
}

