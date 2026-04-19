using Moq;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Chores.CreateChore;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Tests.Features.Chores;

public class CreateChoreCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IProjectRepository> _projectRepositoryMock = new();
    private readonly Mock<IHouseholdChoreRepository> _choreRepositoryMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();

    public CreateChoreCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Projects).Returns(_projectRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Chores).Returns(_choreRepositoryMock.Object);
    }

    private CreateChoreCommandHandler CreateHandler() => new(
        _unitOfWorkMock.Object,
        _currentUserServiceMock.Object,
        _cacheServiceMock.Object);

    private static Project BuildProjectWithMember(long projectId, long userId)
    {
        var project = ApplicationTestUtils.CreateInstanceWithoutConstructor<Project>();
        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.Id), projectId);
        var member = ApplicationTestUtils.CreateInstanceWithoutConstructor<ProjectMember>();
        ApplicationTestUtils.SetPrivatePropertyValue(member, nameof(ProjectMember.UserId), userId);
        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.ProjectMembers),
            new List<ProjectMember> { member });
        return project;
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldCreateChoreAndInvalidateCache()
    {
        const long userId = 10L;
        const long projectId = 42L;
        var command = new CreateChoreCommand(projectId, "Clean kitchen");

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _projectRepositoryMock
            .Setup(r => r.GetProjectWithMembersAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildProjectWithMember(projectId, userId));

        var response = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);

        _choreRepositoryMock.Verify(r => r.AddAsync(It.IsAny<HouseholdChore>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(CacheKeys.ProjectChores(projectId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(new CreateChoreCommand(1L, "Title"), CancellationToken.None));

        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ShouldThrowNotFoundException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(1L);
        _projectRepositoryMock
            .Setup(r => r.GetProjectWithMembersAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().Handle(new CreateChoreCommand(99L, "Title"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenUserNotMember_ShouldThrowAccessDeniedException()
    {
        const long projectId = 42L;
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(10L);

        _projectRepositoryMock
            .Setup(r => r.GetProjectWithMembersAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildProjectWithMember(projectId, userId: 999L));

        await Assert.ThrowsAsync<AccessDeniedException>(() =>
            CreateHandler().Handle(new CreateChoreCommand(projectId, "Title"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithRecurrence_ShouldCreateRecurringChore()
    {
        const long userId = 10L;
        const long projectId = 42L;
        var command = new CreateChoreCommand(
            projectId, "Weekly vacuum",
            RecurrenceFrequency: RecurrenceFrequency.Weekly,
            RecurrenceInterval: 1,
            DueDate: DateTime.UtcNow.Date.AddDays(7));

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _projectRepositoryMock
            .Setup(r => r.GetProjectWithMembersAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildProjectWithMember(projectId, userId));

        var response = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);

        _choreRepositoryMock.Verify(r => r.AddAsync(
            It.Is<HouseholdChore>(c => c.IsRecurring && c.Recurrence != null),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
