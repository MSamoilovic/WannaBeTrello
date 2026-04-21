using Moq;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Bills.MarkBillPaid;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Tests.Features.Bills;

public class MarkBillPaidCommandHandlerTests
{
    private readonly Mock<IBillService> _billServiceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();

    private MarkBillPaidCommandHandler CreateHandler() => new(
        _billServiceMock.Object,
        _currentUserServiceMock.Object,
        _cacheServiceMock.Object);

    [Fact]
    public async Task Handle_WhenValidRequest_DelegatesToServiceAndInvalidatesCache()
    {
        const long userId = 10L;
        const long billId = 1L;
        const long projectId = 42L;

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _billServiceMock
            .Setup(s => s.MarkBillPaidAsync(billId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((projectId, (long?)null));

        var response = await CreateHandler().Handle(new MarkBillPaidCommand(billId), CancellationToken.None);

        Assert.True(response.Result.IsSuccess);
        Assert.Null(response.Result.Value);
        _cacheServiceMock.Verify(c => c.RemoveAsync(CacheKeys.ProjectBills(projectId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RecurringBill_ReturnsNextOccurrenceId()
    {
        const long userId = 10L;
        const long billId = 1L;
        const long projectId = 42L;
        const long nextBillId = 101L;

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _billServiceMock
            .Setup(s => s.MarkBillPaidAsync(billId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((projectId, (long?)nextBillId));

        var response = await CreateHandler().Handle(new MarkBillPaidCommand(billId), CancellationToken.None);

        Assert.True(response.Result.IsSuccess);
        Assert.Equal(nextBillId, response.Result.Value);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_Throws()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(new MarkBillPaidCommand(1L), CancellationToken.None));

        _billServiceMock.Verify(s => s.MarkBillPaidAsync(
            It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
