using Moq;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Household.UpdateHouseholdProfile;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Tests.Features.Household;

public class UpdateHouseholdProfileCommandHandlerTests
{
    private readonly Mock<IHouseholdService> _householdServiceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();

    private UpdateHouseholdProfileCommandHandler CreateHandler() => new(
        _householdServiceMock.Object,
        _currentUserServiceMock.Object,
        _cacheServiceMock.Object);

    private static HouseholdProfile BuildProfile(long id, long projectId)
    {
        var profile = ApplicationTestUtils.CreateInstanceWithoutConstructor<HouseholdProfile>();
        ApplicationTestUtils.SetPrivatePropertyValue(profile, nameof(HouseholdProfile.Id), id);
        ApplicationTestUtils.SetPrivatePropertyValue(profile, nameof(HouseholdProfile.ProjectId), projectId);
        return profile;
    }

    [Fact]
    public async Task Handle_WhenValidRequest_DelegatesToServiceAndInvalidatesCache()
    {
        const long userId = 10L;
        const long projectId = 42L;
        const long profileId = 7L;

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var command = new UpdateHouseholdProfileCommand(projectId, "New address", "Novi Sad", "Europe/Belgrade", DayOfWeek.Sunday);

        _householdServiceMock
            .Setup(s => s.UpdateProfileAsync(projectId, userId, command.Address, command.City,
                command.Timezone, command.ShoppingDay, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildProfile(profileId, projectId));

        var response = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);
        Assert.Equal(profileId, response.Result.Value);
        _cacheServiceMock.Verify(c => c.RemoveAsync(CacheKeys.HouseholdProfile(projectId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);
        var command = new UpdateHouseholdProfileCommand(1L, null, null, null, null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(command, CancellationToken.None));

        _householdServiceMock.Verify(s => s.UpdateProfileAsync(
            It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string?>(), It.IsAny<string?>(),
            It.IsAny<string?>(), It.IsAny<DayOfWeek?>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
