using Moq;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Bills.CreateBill;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Tests.Features.Bills;

public class CreateBillCommandHandlerTests
{
    private readonly Mock<IBillService> _billServiceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();

    private CreateBillCommandHandler CreateHandler() => new(
        _billServiceMock.Object,
        _currentUserServiceMock.Object,
        _cacheServiceMock.Object);

    private static Bill BuildBill(long id, long projectId)
    {
        var bill = ApplicationTestUtils.CreateInstanceWithoutConstructor<Bill>();
        ApplicationTestUtils.SetPrivatePropertyValue(bill, nameof(Bill.Id), id);
        ApplicationTestUtils.SetPrivatePropertyValue(bill, nameof(Bill.ProjectId), projectId);
        return bill;
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldDelegateToServiceAndInvalidateCache()
    {
        const long userId = 10L;
        const long projectId = 42L;
        const long billId = 500L;
        var command = new CreateBillCommand(projectId, "Internet", 50m, DateTime.UtcNow.Date.AddDays(5));

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _billServiceMock
            .Setup(s => s.CreateBillAsync(
                projectId, userId, command.Title, command.Amount, command.DueDate,
                command.Currency, command.Description, command.Category, command.SplitUserIds,
                command.RecurrenceFrequency, command.RecurrenceInterval,
                command.RecurrenceDaysOfWeek, command.RecurrenceEndDate,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildBill(billId, projectId));

        var response = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);
        Assert.Equal(billId, response.Result.Value);

        _cacheServiceMock.Verify(c => c.RemoveAsync(CacheKeys.ProjectBills(projectId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(new CreateBillCommand(1L, "Title", 10m, DateTime.UtcNow.Date.AddDays(1)),
                CancellationToken.None));

        _billServiceMock.Verify(s => s.CreateBillAsync(
            It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<decimal>(),
            It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(),
            It.IsAny<IReadOnlyCollection<long>?>(), It.IsAny<Domain.Enums.RecurrenceFrequency?>(),
            It.IsAny<int>(), It.IsAny<IEnumerable<DayOfWeek>?>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}
