using Moq;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Chores.CompleteChore;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Tests.Features.Chores;

public class CompleteChoreCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IHouseholdChoreRepository> _choreRepositoryMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();

    public CompleteChoreCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Chores).Returns(_choreRepositoryMock.Object);
    }

    private CompleteChoreCommandHandler CreateHandler() => new(
        _unitOfWorkMock.Object,
        _currentUserServiceMock.Object,
        _cacheServiceMock.Object);

    private static HouseholdChore BuildChoreWithProject(long choreId, long projectId, long memberUserId)
    {
        var project = ApplicationTestUtils.CreateInstanceWithoutConstructor<Project>();
        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.Id), projectId);
        var member = ApplicationTestUtils.CreateInstanceWithoutConstructor<ProjectMember>();
        ApplicationTestUtils.SetPrivatePropertyValue(member, nameof(ProjectMember.UserId), memberUserId);
        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.ProjectMembers),
            new List<ProjectMember> { member });

        var chore = HouseholdChore.Create(projectId, "Test chore", memberUserId);
        ApplicationTestUtils.SetPrivatePropertyValue(chore, nameof(HouseholdChore.Id), choreId);
        ApplicationTestUtils.SetPrivatePropertyValue(chore, nameof(HouseholdChore.Project), project);
        return chore;
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldCompleteChore()
    {
        const long userId = 10L;
        const long choreId = 1L;
        const long projectId = 42L;

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var chore = BuildChoreWithProject(choreId, projectId, userId);
        _choreRepositoryMock
            .Setup(r => r.GetByIdAsync(choreId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chore);

        var response = await CreateHandler().Handle(new CompleteChoreCommand(choreId), CancellationToken.None);

        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);
        Assert.Null(response.Result.Value);
        Assert.True(chore.IsCompleted);

        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(CacheKeys.ProjectChores(projectId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(new CompleteChoreCommand(1L), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenChoreNotFound_ShouldThrowNotFoundException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(1L);
        _choreRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((HouseholdChore?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().Handle(new CompleteChoreCommand(99L), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenUserNotMember_ShouldThrowAccessDeniedException()
    {
        const long choreId = 1L;
        const long projectId = 42L;

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(10L);

        var chore = BuildChoreWithProject(choreId, projectId, memberUserId: 999L);
        _choreRepositoryMock
            .Setup(r => r.GetByIdAsync(choreId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chore);

        await Assert.ThrowsAsync<AccessDeniedException>(() =>
            CreateHandler().Handle(new CompleteChoreCommand(choreId), CancellationToken.None));
    }
}
