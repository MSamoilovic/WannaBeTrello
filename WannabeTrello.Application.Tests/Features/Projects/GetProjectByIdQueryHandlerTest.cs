using System.Reflection;
using Moq;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Projects.GetProjectById;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.Projects;

public class GetProjectByIdQueryHandlerTest
{
    private readonly Mock<IProjectService> _projectServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetProjectByIdQueryHandler _handler;

    public GetProjectByIdQueryHandlerTest()
    {
        _projectServiceMock = new Mock<IProjectService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _handler = new GetProjectByIdQueryHandler(_projectServiceMock.Object, _currentUserServiceMock.Object);
    }
    
    [Fact]
    public async Task Handle_AuthenticatedUser_ReturnsProjectDetails()
    {
        // ARRANGE
        const int userId = 123;
        const int projectId = 1;
        
        var mockProject = new Project();
        SetPrivatePropertyValue(mockProject, "Id", projectId);
        SetPrivatePropertyValue(mockProject, "Name", "Test Project");
        
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _projectServiceMock.Setup(x => x.GetProjectByIdAsync(projectId, userId))
                           .ReturnsAsync(mockProject);
        
        var query = new GetProjectByIdQuery(projectId);
        
        // ACT
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // ASSERT
        Assert.NotNull(result);
        Assert.Equal(mockProject.Name, result.Name);
        Assert.Equal(mockProject.Id, result.Id);
        _projectServiceMock.Verify(x => x.GetProjectByIdAsync(projectId, userId), Times.Once);
    }

    [Theory]
    [InlineData(false, null)]
    [InlineData(true, null)]
    public async Task Handle_UnauthenticatedUser_ThrowsAccessDeniedException(bool isAuthenticated, long? userId)
    {
        // ARRANGE
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(isAuthenticated);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        
        var query = new GetProjectByIdQuery(1);
        
        // ACT & ASSERT
        await Assert.ThrowsAsync<AccessDeniedException>(() => _handler.Handle(query, CancellationToken.None));
        _projectServiceMock.Verify(x => x.GetProjectByIdAsync(It.IsAny<long>(), It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ProjectNotFound_ThrowsNotFoundException()
    {
        // ARRANGE
        var userId = 123;
        var projectId = 999;
        
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        
        // Simuliramo da servis baca izuzetak jer projekat nije pronađen
        _projectServiceMock.Setup(x => x.GetProjectByIdAsync(projectId, userId))
                           .ThrowsAsync(new NotFoundException(nameof(Project), projectId));
        
        var query = new GetProjectByIdQuery(projectId);
        
        // ACT & ASSERT
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));
        _projectServiceMock.Verify(x => x.GetProjectByIdAsync(projectId, userId), Times.Once);
    }
    
    private static void SetPrivatePropertyValue<T>(T obj, string propertyName, object value)
    {
        typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(obj, value);
    }
}