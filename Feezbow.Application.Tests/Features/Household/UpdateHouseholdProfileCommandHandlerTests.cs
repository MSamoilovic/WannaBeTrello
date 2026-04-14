using Moq;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Household.UpdateHouseholdProfile;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Tests.Features.Household;

public class UpdateHouseholdProfileCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IHouseholdRepository> _householdRepositoryMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();

    public UpdateHouseholdProfileCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Households).Returns(_householdRepositoryMock.Object);
    }

    private UpdateHouseholdProfileCommandHandler CreateHandler() => new(
        _unitOfWorkMock.Object,
        _currentUserServiceMock.Object,
        _cacheServiceMock.Object);

    private static HouseholdProfile BuildProfile(long projectId, long memberUserId)
    {
        var project = ApplicationTestUtils.CreateInstanceWithoutConstructor<Project>();
        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.Id), projectId);
        var member = ApplicationTestUtils.CreateInstanceWithoutConstructor<ProjectMember>();
        ApplicationTestUtils.SetPrivatePropertyValue(member, nameof(ProjectMember.UserId), memberUserId);
        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.ProjectMembers), new List<ProjectMember> { member });

        var profile = HouseholdProfile.Create(projectId, memberUserId, "Old", "OldCity", "Europe/Belgrade", DayOfWeek.Saturday);
        ApplicationTestUtils.SetPrivatePropertyValue(profile, nameof(HouseholdProfile.Project), project);
        return profile;
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldUpdateProfileAndInvalidateCache()
    {
        const long userId = 10L;
        const long projectId = 42L;
        var profile = BuildProfile(projectId, userId);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _householdRepositoryMock
            .Setup(r => r.GetByProjectIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var command = new UpdateHouseholdProfileCommand(projectId, "New address", "Novi Sad", "Europe/Belgrade", DayOfWeek.Sunday);

        var response = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);
        Assert.Equal("New address", profile.Address);
        Assert.Equal("Novi Sad", profile.City);
        Assert.Equal(DayOfWeek.Sunday, profile.ShoppingDay);

        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(CacheKeys.HouseholdProfile(projectId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);
        var command = new UpdateHouseholdProfileCommand(1L, null, null, null, null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenProfileNotFound_ShouldThrowNotFoundException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(1L);

        _householdRepositoryMock
            .Setup(r => r.GetByProjectIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((HouseholdProfile?)null);

        var command = new UpdateHouseholdProfileCommand(99L, null, null, null, null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenUserNotMember_ShouldThrowAccessDeniedException()
    {
        const long projectId = 42L;
        var profile = BuildProfile(projectId, memberUserId: 999L);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(10L);

        _householdRepositoryMock
            .Setup(r => r.GetByProjectIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var command = new UpdateHouseholdProfileCommand(projectId, "x", null, null, null);

        await Assert.ThrowsAsync<AccessDeniedException>(() =>
            CreateHandler().Handle(command, CancellationToken.None));

        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
