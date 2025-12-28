using System.Runtime.Serialization;
using Moq;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Users.GetCurrentUserProfile;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.Users;

public class GetCurrentUserProfileQueryHandlerTests
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
    public async Task Handle_WhenUserIsAuthenticatedAndProfileExists_ShouldReturnCurrentUserProfile()
    {
        // Arrange
        const long userId = 123L;
        var query = new GetCurrentUserProfileQuery();

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
        SetPrivatePropertyValue(userFromService, nameof(User.IsActive), true);
        SetPrivatePropertyValue(userFromService, nameof(User.CreatedAt), DateTime.UtcNow);
        SetPrivatePropertyValue(userFromService, nameof(User.PhoneNumber), "+123456789");
        
        // Initialize collections
        SetPrivateFieldValue(userFromService, "_ownedProjects", new List<Project>());
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

        var handler = new GetCurrentUserProfileQueryHandler(userServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act
        var response = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(userId, response.Id);
        Assert.Equal("testuser", response.UserName);
        Assert.Equal("test@example.com", response.Email);
        Assert.Equal("Test", response.FirstName);
        Assert.Equal("User", response.LastName);
        Assert.Equal("Test bio", response.Bio);
        Assert.Equal("+123456789", response.PhoneNumber);
        Assert.True(response.IsActive);
        Assert.NotNull(response.CreatedProjects);
        Assert.NotNull(response.TasksAssigned);
        Assert.NotNull(response.ProjectMemberships);

        userServiceMock.Verify(s => s.GetUserProfileAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        cacheServiceMock.Verify(c => c.GetOrSetAsync(
            It.Is<string>(k => k == CacheKeys.UserProfile(userId)),
            It.IsAny<Func<Task<User?>>>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var query = new GetCurrentUserProfileQuery();
        
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var userServiceMock = new Mock<IUserService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new GetCurrentUserProfileQueryHandler(userServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

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
        var query = new GetCurrentUserProfileQuery();
        
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        var userServiceMock = new Mock<IUserService>();
        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new GetCurrentUserProfileQueryHandler(userServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            handler.Handle(query, CancellationToken.None));
        
        Assert.Equal("User is not authenticated", exception.Message);
        userServiceMock.Verify(s => s.GetUserProfileAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WhenUserProfileNotFound_ShouldReturnNull()
    {
        // Arrange
        const long userId = 999L;
        var query = new GetCurrentUserProfileQuery();

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var userServiceMock = new Mock<IUserService>();
        userServiceMock
            .Setup(s => s.GetUserProfileAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var cacheServiceMock = new Mock<ICacheService>();
        var handler = new GetCurrentUserProfileQueryHandler(userServiceMock.Object, currentUserServiceMock.Object, cacheServiceMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() =>
            handler.Handle(query, CancellationToken.None));
    }
}

