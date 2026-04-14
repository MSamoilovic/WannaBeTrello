using Moq;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Household.CreateHouseholdProfile;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Tests.Features.Household;

public class CreateHouseholdProfileCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IProjectRepository> _projectRepositoryMock = new();
    private readonly Mock<IHouseholdRepository> _householdRepositoryMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();

    public CreateHouseholdProfileCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Projects).Returns(_projectRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Households).Returns(_householdRepositoryMock.Object);
    }

    private CreateHouseholdProfileCommandHandler CreateHandler() => new(
        _unitOfWorkMock.Object,
        _currentUserServiceMock.Object,
        _cacheServiceMock.Object);

    private static Project BuildProjectWithMember(long projectId, long userId)
    {
        var project = ApplicationTestUtils.CreateInstanceWithoutConstructor<Project>();
        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.Id), projectId);
        var member = ApplicationTestUtils.CreateInstanceWithoutConstructor<ProjectMember>();
        ApplicationTestUtils.SetPrivatePropertyValue(member, nameof(ProjectMember.UserId), userId);
        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.ProjectMembers), new List<ProjectMember> { member });
        return project;
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldCreateProfileAndInvalidateCache()
    {
        const long userId = 10L;
        const long projectId = 42L;
        var command = new CreateHouseholdProfileCommand(projectId, "Some street 1", "Belgrade", "Europe/Belgrade", DayOfWeek.Friday);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _projectRepositoryMock
            .Setup(r => r.GetProjectWithMembersAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildProjectWithMember(projectId, userId));

        _householdRepositoryMock
            .Setup(r => r.ExistsForProjectAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var response = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);

        _householdRepositoryMock.Verify(r => r.AddAsync(It.IsAny<HouseholdProfile>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(CacheKeys.HouseholdProfile(projectId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);
        var command = new CreateHouseholdProfileCommand(1L, null, null, null, null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(command, CancellationToken.None));

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

        var command = new CreateHouseholdProfileCommand(99L, null, null, null, null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenUserNotMemberOfProject_ShouldThrowAccessDeniedException()
    {
        const long userId = 10L;
        const long projectId = 42L;

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _projectRepositoryMock
            .Setup(r => r.GetProjectWithMembersAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildProjectWithMember(projectId, userId: 999L));

        var command = new CreateHouseholdProfileCommand(projectId, null, null, null, null);

        await Assert.ThrowsAsync<AccessDeniedException>(() =>
            CreateHandler().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenProfileAlreadyExists_ShouldThrowInvalidOperationDomainException()
    {
        const long userId = 10L;
        const long projectId = 42L;

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _projectRepositoryMock
            .Setup(r => r.GetProjectWithMembersAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildProjectWithMember(projectId, userId));

        _householdRepositoryMock
            .Setup(r => r.ExistsForProjectAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new CreateHouseholdProfileCommand(projectId, null, null, null, null);

        await Assert.ThrowsAsync<InvalidOperationDomainException>(() =>
            CreateHandler().Handle(command, CancellationToken.None));

        _householdRepositoryMock.Verify(r => r.AddAsync(It.IsAny<HouseholdProfile>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
