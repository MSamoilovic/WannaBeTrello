using Moq;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Projects.ArchiveProject;
using WannabeTrello.Application.Tests.Utils;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.Projects;

public class ArchiveProjectCommandTests
{
    private readonly Mock<IProjectService> _projectServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly ArchiveProjectCommandHandler _handler;

    public ArchiveProjectCommandTests()
    {
        _projectServiceMock = new Mock<IProjectService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _handler = new ArchiveProjectCommandHandler(
            _projectServiceMock.Object,
            _currentUserServiceMock.Object,
            _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_AuthenticatedUser_ReturnsSuccessfulResult()
    {
        // ARRANGE
        const int projectId = 1;
        const int userId = 123;
        
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        
        // Project sa OwnerId (za invalidaciju keša)
        var project = ApplicationTestUtils.CreateInstanceWithoutConstructor<Project>();
        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.Id), projectId);
        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.OwnerId), 456L);
        
        _projectServiceMock.Setup(x => x.GetProjectByIdAsync(projectId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);
        
        _projectServiceMock.Setup(x => x.ArchiveProjectAsync(projectId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(projectId);

        var command = new ArchiveProjectCommand(projectId);
        
        // ACT
        var response = await _handler.Handle(command, CancellationToken.None);
       
        // ASSERT
        
        Assert.NotNull(response);
        Assert.NotNull(response.Result);
        Assert.True(response.Result.IsSuccess);
        Assert.Equal(projectId, response.Result.Value);
        Assert.Equal($"Project {projectId} is now archived.", response.Result.Message);

        // Verify the service method was called exactly once with the correct parameters
        _projectServiceMock.Verify(x => x.ArchiveProjectAsync(projectId, userId, It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(It.Is<string>(k => k == CacheKeys.Project(projectId)), It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(It.Is<string>(k => k == CacheKeys.UserProjects(456L)), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Theory]
    [InlineData(false, null)]
    [InlineData(true, null)]
    public async Task Handle_UnauthenticatedOrMissingUser_ThrowsUnauthorizedAccessException(bool isAuthenticated, long? userId)
    {
        // ARRANGE
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(isAuthenticated);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        
        var command = new ArchiveProjectCommand(1);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _handler.Handle(command, CancellationToken.None));
        
        _projectServiceMock.Verify(x => x.ArchiveProjectAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_ProjectNotFound_ThrowsNotFoundException()
    {
        // ARRANGE
        const int projectId = 999;
        const int userId = 123;
    
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
    
        // Mock the service to throw a NotFoundException
        _projectServiceMock.Setup(x => x.ArchiveProjectAsync(projectId, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(nameof(Project), projectId));

        var command = new ArchiveProjectCommand(projectId);

        // ACT & ASSERT
       
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal($"Entity \'{nameof(Project)}\' ({projectId}) was not found.", exception.Message);
        
        _projectServiceMock.Verify(x => x.ArchiveProjectAsync(projectId, userId, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_UserLacksPermissions_ThrowsUnauthorizedAccessException()
    {
        // ARRANGE
        const int projectId = 1;
        const int userId = 123;
    
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        
        _projectServiceMock.Setup(x => x.ArchiveProjectAsync(projectId, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Only Owner or Admin can archive this project."));

        var command = new ArchiveProjectCommand(projectId);

        // ACT & ASSERT
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _handler.Handle(command, CancellationToken.None));
        
        _projectServiceMock.Verify(x => x.ArchiveProjectAsync(projectId, userId, It.IsAny<CancellationToken>()), Times.Once);
    }
}