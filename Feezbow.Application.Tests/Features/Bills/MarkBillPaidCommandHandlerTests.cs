using Moq;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Bills.MarkBillPaid;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;
using Feezbow.Domain.ValueObjects;

namespace Feezbow.Application.Tests.Features.Bills;

public class MarkBillPaidCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IBillRepository> _billRepositoryMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();

    public MarkBillPaidCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Bills).Returns(_billRepositoryMock.Object);
    }

    private MarkBillPaidCommandHandler CreateHandler() => new(
        _unitOfWorkMock.Object,
        _currentUserServiceMock.Object,
        _cacheServiceMock.Object);

    private static Bill BuildBillWithProject(long billId, long projectId, long memberUserId,
        RecurrenceRule? recurrence = null)
    {
        var project = ApplicationTestUtils.CreateInstanceWithoutConstructor<Project>();
        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.Id), projectId);
        var member = ApplicationTestUtils.CreateInstanceWithoutConstructor<ProjectMember>();
        ApplicationTestUtils.SetPrivatePropertyValue(member, nameof(ProjectMember.UserId), memberUserId);
        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.ProjectMembers),
            new List<ProjectMember> { member });

        var bill = Bill.Create(projectId, "Test bill", 100m, DateTime.UtcNow.Date.AddDays(1), memberUserId,
            recurrence: recurrence);
        ApplicationTestUtils.SetPrivatePropertyValue(bill, nameof(Bill.Id), billId);
        ApplicationTestUtils.SetPrivatePropertyValue(bill, nameof(Bill.Project), project);
        return bill;
    }

    [Fact]
    public async Task Handle_WhenValidRequest_MarksBillPaid()
    {
        const long userId = 10L;
        const long billId = 1L;
        const long projectId = 42L;

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var bill = BuildBillWithProject(billId, projectId, userId);
        _billRepositoryMock
            .Setup(r => r.GetByIdAsync(billId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bill);

        var response = await CreateHandler().Handle(new MarkBillPaidCommand(billId), CancellationToken.None);

        Assert.True(response.Result.IsSuccess);
        Assert.Null(response.Result.Value);
        Assert.True(bill.IsPaid);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(CacheKeys.ProjectBills(projectId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RecurringBill_AddsNextOccurrence()
    {
        const long userId = 10L;
        const long billId = 1L;
        const long projectId = 42L;

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var bill = BuildBillWithProject(billId, projectId, userId,
            RecurrenceRule.Create(RecurrenceFrequency.Monthly));

        _billRepositoryMock
            .Setup(r => r.GetByIdAsync(billId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bill);

        await CreateHandler().Handle(new MarkBillPaidCommand(billId), CancellationToken.None);

        _billRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Bill>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_Throws()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(new MarkBillPaidCommand(1L), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenBillNotFound_ThrowsNotFound()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(1L);
        _billRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Bill?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().Handle(new MarkBillPaidCommand(99L), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenUserNotMember_ThrowsAccessDenied()
    {
        const long billId = 1L;
        const long projectId = 42L;

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(10L);

        var bill = BuildBillWithProject(billId, projectId, memberUserId: 999L);
        _billRepositoryMock
            .Setup(r => r.GetByIdAsync(billId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bill);

        await Assert.ThrowsAsync<AccessDeniedException>(() =>
            CreateHandler().Handle(new MarkBillPaidCommand(billId), CancellationToken.None));
    }
}
