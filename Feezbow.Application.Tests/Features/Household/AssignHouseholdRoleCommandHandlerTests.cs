using Moq;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Household.AssignHouseholdRole;
using Feezbow.Domain.Enums;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Tests.Features.Household;

public class AssignHouseholdRoleCommandHandlerTests
{
    private readonly Mock<IHouseholdService> _householdServiceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();

    private AssignHouseholdRoleCommandHandler CreateHandler() => new(
        _householdServiceMock.Object,
        _currentUserServiceMock.Object,
        _cacheServiceMock.Object);

    [Fact]
    public async Task Handle_WhenValidRequest_DelegatesToServiceAndInvalidatesCache()
    {
        const long userId = 10L;
        const long memberId = 20L;
        const long projectId = 42L;
        var command = new AssignHouseholdRoleCommand(projectId, memberId, HouseholdRole.Adult);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var response = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);

        _householdServiceMock.Verify(s => s.AssignRoleAsync(
            projectId, memberId, HouseholdRole.Adult, userId, It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(CacheKeys.HouseholdMembers(projectId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(new AssignHouseholdRoleCommand(1L, 2L, HouseholdRole.Adult), CancellationToken.None));

        _householdServiceMock.Verify(s => s.AssignRoleAsync(
            It.IsAny<long>(), It.IsAny<long>(), It.IsAny<HouseholdRole>(), It.IsAny<long>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}
