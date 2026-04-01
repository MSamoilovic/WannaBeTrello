using Moq;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Projects.RemoveProjectMember;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;
using Feezbow.Domain.Events;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;
using Feezbow.Domain.Services;
using Feezbow.Domain.ValueObjects;

namespace Feezbow.Application.Tests.Features.Projects;

public class RemoveProjectMemberCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var projectRepoMock = new Mock<IProjectRepository>();
        var userRepoMock = new Mock<IUserRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var projectService = new ProjectService(projectRepoMock.Object, userRepoMock.Object, unitOfWorkMock.Object);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(false);

        var cacheServiceMock = new Mock<ICacheService>();

        var handler = new RemoveProjectMemberCommandHandler(
            projectService,
            currentUserServiceMock.Object,
            cacheServiceMock.Object);

        var command = new RemoveProjectMemberCommand(1L, 2L);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));

        projectRepoMock.Verify(x => x.GetByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
        cacheServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var projectRepoMock = new Mock<IProjectRepository>();
        var userRepoMock = new Mock<IUserRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var projectService = new ProjectService(projectRepoMock.Object, userRepoMock.Object, unitOfWorkMock.Object);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(x => x.UserId).Returns((long?)null);

        var cacheServiceMock = new Mock<ICacheService>();

        var handler = new RemoveProjectMemberCommandHandler(
            projectService,
            currentUserServiceMock.Object,
            cacheServiceMock.Object);

        var command = new RemoveProjectMemberCommand(1L, 2L);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));

        projectRepoMock.Verify(x => x.GetByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
        cacheServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var projectId = 100L;
        var userToRemoveId = 2L;
        var removingUserId = 10L;

        var projectRepoMock = new Mock<IProjectRepository>();
        projectRepoMock
            .Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        var userRepoMock = new Mock<IUserRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var projectService = new ProjectService(projectRepoMock.Object, userRepoMock.Object, unitOfWorkMock.Object);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(x => x.UserId).Returns(removingUserId);

        var cacheServiceMock = new Mock<ICacheService>();

        var handler = new RemoveProjectMemberCommandHandler(
            projectService,
            currentUserServiceMock.Object,
            cacheServiceMock.Object);

        var command = new RemoveProjectMemberCommand(projectId, userToRemoveId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None));

        projectRepoMock.Verify(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()), Times.Once);
        cacheServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRemovalSucceeds_ShouldInvalidateProjectMembersCache()
    {
        // Arrange
        var projectId = 1L;
        var userToRemoveId = 2L;
        var removingUserId = 10L;

        var project = ApplicationTestUtils.CreateInstanceWithoutConstructor<Project>();
        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.Id), projectId);
        ApplicationTestUtils.SetPrivatePropertyValue(project, "_domainEvents", new List<DomainEvent>());
        ApplicationTestUtils.SetPrivatePropertyValue(project, "_activities", new List<Activity>());

        var projectMembers = new List<ProjectMember>();

        var removerMember = ApplicationTestUtils.CreateInstanceWithoutConstructor<ProjectMember>();
        ApplicationTestUtils.SetPrivatePropertyValue(removerMember, "UserId", removingUserId);
        ApplicationTestUtils.SetPrivatePropertyValue(removerMember, "Role", ProjectRole.Admin);

        var memberToRemove = ApplicationTestUtils.CreateInstanceWithoutConstructor<ProjectMember>();
        ApplicationTestUtils.SetPrivatePropertyValue(memberToRemove, "UserId", userToRemoveId);
        ApplicationTestUtils.SetPrivatePropertyValue(memberToRemove, "Role", ProjectRole.Contributor);

        projectMembers.Add(removerMember);
        projectMembers.Add(memberToRemove);
        ApplicationTestUtils.SetPrivatePropertyValue(project, "ProjectMembers", projectMembers);

        var projectRepoMock = new Mock<IProjectRepository>();
        projectRepoMock
            .Setup(x => x.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);
        projectRepoMock.Setup(x => x.Update(project));

        var userRepoMock = new Mock<IUserRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock
            .Setup(x => x.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var projectService = new ProjectService(projectRepoMock.Object, userRepoMock.Object, unitOfWorkMock.Object);

        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(x => x.UserId).Returns(removingUserId);

        var cacheServiceMock = new Mock<ICacheService>();
        cacheServiceMock
            .Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new RemoveProjectMemberCommandHandler(
            projectService,
            currentUserServiceMock.Object,
            cacheServiceMock.Object);

        var command = new RemoveProjectMemberCommand(projectId, userToRemoveId);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);

        cacheServiceMock.Verify(
            x => x.RemoveAsync(CacheKeys.ProjectMembers(projectId), It.IsAny<CancellationToken>()),
            Times.Once);
        unitOfWorkMock.Verify(x => x.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
