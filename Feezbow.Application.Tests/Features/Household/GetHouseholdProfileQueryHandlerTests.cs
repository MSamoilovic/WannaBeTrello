using Moq;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Household.GetHouseholdProfile;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Tests.Features.Household;

public class GetHouseholdProfileQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IHouseholdRepository> _householdRepositoryMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();

    public GetHouseholdProfileQueryHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Households).Returns(_householdRepositoryMock.Object);
        _cacheServiceMock
            .Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<GetHouseholdProfileQueryResponse>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<GetHouseholdProfileQueryResponse>>, TimeSpan?, CancellationToken>(
                (_, factory, _, _) => factory());
    }

    private GetHouseholdProfileQueryHandler CreateHandler() => new(
        _unitOfWorkMock.Object,
        _currentUserServiceMock.Object,
        _cacheServiceMock.Object);

    private static HouseholdProfile BuildProfile(long projectId, long memberUserId)
    {
        var project = ApplicationTestUtils.CreateInstanceWithoutConstructor<Project>();
        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.Id), projectId);
        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.Name), "Home");
        var member = ApplicationTestUtils.CreateInstanceWithoutConstructor<ProjectMember>();
        ApplicationTestUtils.SetPrivatePropertyValue(member, nameof(ProjectMember.UserId), memberUserId);
        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.ProjectMembers), new List<ProjectMember> { member });

        var profile = HouseholdProfile.Create(projectId, memberUserId, "Addr", "City", "Europe/Belgrade", DayOfWeek.Monday);
        ApplicationTestUtils.SetPrivatePropertyValue(profile, nameof(HouseholdProfile.Project), project);
        return profile;
    }

    [Fact]
    public async Task Handle_WhenProfileExistsAndUserIsMember_ShouldReturnResponse()
    {
        const long userId = 10L;
        const long projectId = 42L;

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _householdRepositoryMock
            .Setup(r => r.GetByProjectIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildProfile(projectId, userId));

        var response = await CreateHandler().Handle(new GetHouseholdProfileQuery(projectId), CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal(projectId, response.ProjectId);
        Assert.Equal("Home", response.ProjectName);
        Assert.Equal("Addr", response.Address);
        Assert.Equal("City", response.City);
        Assert.Equal(DayOfWeek.Monday.ToString(), response.ShoppingDay);

        _cacheServiceMock.Verify(c => c.GetOrSetAsync(
            It.Is<string>(k => k == CacheKeys.HouseholdProfile(projectId)),
            It.IsAny<Func<Task<GetHouseholdProfileQueryResponse>>>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(new GetHouseholdProfileQuery(1L), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenProfileNotFound_ShouldThrowNotFoundException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(1L);
        _householdRepositoryMock
            .Setup(r => r.GetByProjectIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((HouseholdProfile?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().Handle(new GetHouseholdProfileQuery(99L), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenUserNotMember_ShouldThrowAccessDeniedException()
    {
        const long projectId = 42L;
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(10L);

        _householdRepositoryMock
            .Setup(r => r.GetByProjectIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildProfile(projectId, memberUserId: 999L));

        await Assert.ThrowsAsync<AccessDeniedException>(() =>
            CreateHandler().Handle(new GetHouseholdProfileQuery(projectId), CancellationToken.None));
    }
}
