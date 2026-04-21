using Moq;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Household.GetHouseholdMembers;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Tests.Features.Household;

public class GetHouseholdMembersQueryHandlerTests
{
    private readonly Mock<IHouseholdService> _householdServiceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();

    private GetHouseholdMembersQueryHandler CreateHandler() => new(
        _householdServiceMock.Object,
        _currentUserServiceMock.Object,
        _cacheServiceMock.Object);

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(new GetHouseholdMembersQuery(1L), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenProfileNotFound_ShouldThrowNotFoundException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(1L);

        _cacheServiceMock
            .Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<GetHouseholdMembersQueryResponse>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<Task<GetHouseholdMembersQueryResponse>>, TimeSpan?, CancellationToken>(
                (_, factory, _, _) => factory());

        _householdServiceMock
            .Setup(s => s.GetMembersAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException(nameof(HouseholdProfile), 99L));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().Handle(new GetHouseholdMembersQuery(99L), CancellationToken.None));
    }
}
