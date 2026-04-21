using Moq;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Household.GetHouseholdProfile;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Tests.Features.Household;

public class GetHouseholdProfileQueryHandlerTests
{
    private readonly Mock<IHouseholdService> _householdServiceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();

    public GetHouseholdProfileQueryHandlerTests()
    {
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
        _householdServiceMock.Object,
        _currentUserServiceMock.Object,
        _cacheServiceMock.Object);

    private static HouseholdProfile BuildProfile(long projectId)
    {
        var project = ApplicationTestUtils.CreateInstanceWithoutConstructor<Project>();
        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.Id), projectId);
        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.Name), "Home");

        var profile = HouseholdProfile.Create(projectId, 1L, "Addr", "City", "Europe/Belgrade", DayOfWeek.Monday);
        ApplicationTestUtils.SetPrivatePropertyValue(profile, nameof(HouseholdProfile.Project), project);
        return profile;
    }

    [Fact]
    public async Task Handle_WhenProfileExists_ShouldReturnResponse()
    {
        const long userId = 10L;
        const long projectId = 42L;

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _householdServiceMock
            .Setup(s => s.GetProfileAsync(projectId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildProfile(projectId));

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
    public async Task Handle_WhenServiceThrowsAccessDenied_PropagatesException()
    {
        const long projectId = 42L;
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(10L);

        _householdServiceMock
            .Setup(s => s.GetProfileAsync(projectId, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AccessDeniedException("Not a member"));

        await Assert.ThrowsAsync<AccessDeniedException>(() =>
            CreateHandler().Handle(new GetHouseholdProfileQuery(projectId), CancellationToken.None));
    }
}
