using Moq;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Household.AssignHouseholdRole;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Tests.Features.Household;

public class AssignHouseholdRoleCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IHouseholdRepository> _householdRepositoryMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();

    public AssignHouseholdRoleCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Households).Returns(_householdRepositoryMock.Object);
    }

    private AssignHouseholdRoleCommandHandler CreateHandler() => new(
        _unitOfWorkMock.Object,
        _currentUserServiceMock.Object,
        _cacheServiceMock.Object);

    private static (HouseholdProfile profile, ProjectMember member) BuildProfileWithMember(
        long projectId, long callerUserId, long targetMemberId)
    {
        var project = ApplicationTestUtils.CreateInstanceWithoutConstructor<Project>();
        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.Id), projectId);

        var callerMember = ApplicationTestUtils.CreateInstanceWithoutConstructor<ProjectMember>();
        ApplicationTestUtils.SetPrivatePropertyValue(callerMember, nameof(ProjectMember.UserId), callerUserId);
        ApplicationTestUtils.SetPrivatePropertyValue(callerMember, nameof(ProjectMember.ProjectId), projectId);

        var targetMember = ApplicationTestUtils.CreateInstanceWithoutConstructor<ProjectMember>();
        ApplicationTestUtils.SetPrivatePropertyValue(targetMember, nameof(ProjectMember.UserId), targetMemberId);
        ApplicationTestUtils.SetPrivatePropertyValue(targetMember, nameof(ProjectMember.ProjectId), projectId);

        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.ProjectMembers),
            new List<ProjectMember> { callerMember, targetMember });

        var profile = ApplicationTestUtils.CreateInstanceWithoutConstructor<HouseholdProfile>();
        ApplicationTestUtils.SetPrivatePropertyValue(profile, nameof(HouseholdProfile.ProjectId), projectId);
        ApplicationTestUtils.SetPrivatePropertyValue(profile, nameof(HouseholdProfile.Project), project);
        ApplicationTestUtils.InitializeDomainEvents(profile);

        return (profile, targetMember);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldAssignRoleAndInvalidateCache()
    {
        const long userId = 10L;
        const long memberId = 20L;
        const long projectId = 42L;
        var command = new AssignHouseholdRoleCommand(projectId, memberId, HouseholdRole.Adult);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var (profile, _) = BuildProfileWithMember(projectId, userId, memberId);
        _householdRepositoryMock
            .Setup(r => r.GetByProjectIdWithMembersAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var response = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);

        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(CacheKeys.HouseholdMembers(projectId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(new AssignHouseholdRoleCommand(1L, 2L, HouseholdRole.Adult), CancellationToken.None));

        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenProfileNotFound_ShouldThrowNotFoundException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(1L);
        _householdRepositoryMock
            .Setup(r => r.GetByProjectIdWithMembersAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((HouseholdProfile?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().Handle(new AssignHouseholdRoleCommand(99L, 2L, HouseholdRole.Adult), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenUserNotMember_ShouldThrowAccessDeniedException()
    {
        const long projectId = 42L;
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(10L);

        var (profile, _) = BuildProfileWithMember(projectId, callerUserId: 999L, targetMemberId: 20L);
        _householdRepositoryMock
            .Setup(r => r.GetByProjectIdWithMembersAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        await Assert.ThrowsAsync<AccessDeniedException>(() =>
            CreateHandler().Handle(new AssignHouseholdRoleCommand(projectId, 20L, HouseholdRole.Adult), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenMemberNotFound_ShouldThrowNotFoundException()
    {
        const long projectId = 42L;
        const long userId = 10L;
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var (profile, _) = BuildProfileWithMember(projectId, userId, targetMemberId: 20L);
        _householdRepositoryMock
            .Setup(r => r.GetByProjectIdWithMembersAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().Handle(new AssignHouseholdRoleCommand(projectId, 999L, HouseholdRole.Adult), CancellationToken.None));
    }
}
