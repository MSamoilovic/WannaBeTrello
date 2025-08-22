using Moq;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Projects.AddProjectMember;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Exceptions;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.Projects;

public class AddProjectMemberTests
{
    private readonly Mock<IProjectService> _projectServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly AddProjectMemberCommandHandler _handler;

    public AddProjectMemberTests()
    {
        _projectServiceMock = new Mock<IProjectService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _handler = new AddProjectMemberCommandHandler(
            _projectServiceMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_AuthenticatedUser_AddsMemberSuccessfully()
    {
        // ARRANGE
        const int projectId = 1;
        const int newMemberId = 2;
        const int inviterUserId = 123;
        const ProjectRole role = ProjectRole.Contributor;
        
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(inviterUserId);
        
        _projectServiceMock.Setup(x => x.AddProjectMember(projectId, newMemberId, role, inviterUserId))
            .ReturnsAsync(projectId);

        var command = new AddProjectMemberCommand(projectId, newMemberId, role);

        // ACT
        var response = await _handler.Handle(command, CancellationToken.None);

        // ASSERT
        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);
        Assert.Equal(projectId, response.Result.Value);
        Assert.Equal($"{newMemberId} is now added to the project.", response.Result.Message);

        _projectServiceMock.Verify(x => x.AddProjectMember(projectId, newMemberId, role, inviterUserId), Times.Once);
    }
    
    
    [Theory]
    [InlineData(false, null)]
    [InlineData(true, null)]
    public async Task Handle_UnauthenticatedOrMissingUser_ThrowsUnauthorizedAccessException(bool isAuthenticated, long? userId)
    {
        // ARRANGE
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(isAuthenticated);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        
        var command = new AddProjectMemberCommand(1, 2, ProjectRole.Contributor);

        // ACT & ASSERT
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _handler.Handle(command, CancellationToken.None));

        _projectServiceMock.Verify(x => x.AddProjectMember(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<ProjectRole>(), It.IsAny<long>()), Times.Never);
    }
    
    
    [Fact]
    public async Task Handle_ProjectNotFound_ThrowsNotFoundException()
    {
        // ARRANGE
        var projectId = 999;
        var userId = 123;
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        _projectServiceMock.Setup(x => x.AddProjectMember(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<ProjectRole>(), It.IsAny<long>()))
            .ThrowsAsync(new NotFoundException(nameof(Project), projectId));

        var command = new AddProjectMemberCommand(projectId, 2, ProjectRole.Contributor);

        // ACT & ASSERT
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));

        _projectServiceMock.Verify(x => x.AddProjectMember(projectId, It.IsAny<long>(), It.IsAny<ProjectRole>(), It.IsAny<long>()), Times.Once);
    }
    

    
    [Fact]
    public async Task Handle_InviterLacksPermissions_ThrowsUnauthorizedAccessException()
    {
        // ARRANGE
        var projectId = 1;
        var userId = 123;
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        _projectServiceMock.Setup(x => x.AddProjectMember(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<ProjectRole>(), It.IsAny<long>()))
            .ThrowsAsync(new UnauthorizedAccessException("Only Admin or Owner can add a new member."));

        var command = new AddProjectMemberCommand(projectId, 2, ProjectRole.Contributor);

        // ACT & ASSERT
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _handler.Handle(command, CancellationToken.None));

        _projectServiceMock.Verify(x => x.AddProjectMember(projectId, It.IsAny<long>(), It.IsAny<ProjectRole>(), It.IsAny<long>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_NewMemberAlreadyExists_ThrowsInvalidOperationException()
    {
        // ARRANGE
        var projectId = 1;
        var userId = 123;
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        _projectServiceMock.Setup(x => x.AddProjectMember(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<ProjectRole>(), It.IsAny<long>()))
            .ThrowsAsync(new InvalidOperationException("This user is already a member of the project."));

        var command = new AddProjectMemberCommand(projectId, 2, ProjectRole.Contributor);

        // ACT & ASSERT
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));

        _projectServiceMock.Verify(x => x.AddProjectMember(projectId, It.IsAny<long>(), It.IsAny<ProjectRole>(), It.IsAny<long>()), Times.Once);
    }
}