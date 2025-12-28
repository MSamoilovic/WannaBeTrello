using Moq;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Projects.UpdateProjectMemberRole;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.Projects;

public class UpdateProjectMemberRoleCommandHandlerTests
{
    private readonly Mock<IProjectService> _projectServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly UpdateProjectMemberRoleCommandHandler _handler;

    public UpdateProjectMemberRoleCommandHandlerTests()
    {
        _projectServiceMock = new Mock<IProjectService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _cacheServiceMock = new Mock<ICacheService>();

        _handler = new UpdateProjectMemberRoleCommandHandler(
            _projectServiceMock.Object,
            _currentUserServiceMock.Object,
            _cacheServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_UserNotAuthenticated_ThrowsUnauthorizedAccessException()
    {
        // ARRANGE
        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(false);
        _currentUserServiceMock.Setup(c => c.UserId).Returns((long?)null);

        const int projectId = 1;
        const int memberId = 2;
        const ProjectRole role = ProjectRole.Contributor;

        var command = new UpdateProjectMemberRoleCommand(projectId, memberId, role);
        
        // ACT & ASSERT
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UserAuthenticated_CallsUpdateProjectMember()
    {
        // ARRANGE
        const long userId = 10;
        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(c => c.UserId).Returns(userId);
        
        const int projectId = 1;
        const int memberId = 2;
        const ProjectRole role = ProjectRole.Contributor;

        var command = new UpdateProjectMemberRoleCommand(projectId, memberId, role);

        // ACT
        var result = await _handler.Handle(command, CancellationToken.None);

        // ASSERT
        _projectServiceMock.Verify(s => s.UpdateProjectMember(
            command.ProjectId,
            command.MemberId,
            command.Role,
            userId,
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.NotNull(result);
        Assert.Equal(command.ProjectId, result.Result.Value);
        Assert.Contains("role updated", result.Result.Message, StringComparison.OrdinalIgnoreCase);
        _cacheServiceMock.Verify(c => c.RemoveAsync(It.Is<string>(k => k == CacheKeys.ProjectMembers(command.ProjectId)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserAuthenticated_ProjectServiceThrowsException_PropagatesException()
    {
        // ARRANGE
        const long userId = 10;
        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(c => c.UserId).Returns(userId);

        _projectServiceMock.Setup(s => s.UpdateProjectMember(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<ProjectRole>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Project not found"));
        
        const int projectId = 99;
        const int memberId = 2;
        const ProjectRole role = ProjectRole.Admin;

        var command = new UpdateProjectMemberRoleCommand(projectId, memberId, role);


        // ACT & ASSERT
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
    
    [Fact]
    public async Task Handle_AdminChangesMemberRole_Succeeds()
    {
        // ARRANGE
        const long userId = 10;
        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(c => c.UserId).Returns(userId);

        const int projectId = 1;
        const int memberId = 2;
        const ProjectRole role = ProjectRole.Contributor;

        var command = new UpdateProjectMemberRoleCommand(projectId, memberId, role);

        // ACT
        var result = await _handler.Handle(command, CancellationToken.None);

        // ASSERT
        _projectServiceMock.Verify(s => s.UpdateProjectMember(
            command.ProjectId,
            command.MemberId,
            command.Role,
            userId,
            It.IsAny<CancellationToken>()
        ), Times.Once);

        Assert.NotNull(result);
        Assert.Equal(command.ProjectId, result.Result.Value);
        Assert.Contains("role updated", result.Result.Message, StringComparison.OrdinalIgnoreCase);
        _cacheServiceMock.Verify(c => c.RemoveAsync(It.Is<string>(k => k == CacheKeys.ProjectMembers(command.ProjectId)), It.IsAny<CancellationToken>()), Times.Once);
    }
}