using Moq;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Bills.CreateBill;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Tests.Features.Bills;

public class CreateBillCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IProjectRepository> _projectRepositoryMock = new();
    private readonly Mock<IBillRepository> _billRepositoryMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();

    public CreateBillCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Projects).Returns(_projectRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Bills).Returns(_billRepositoryMock.Object);
    }

    private CreateBillCommandHandler CreateHandler() => new(
        _unitOfWorkMock.Object,
        _currentUserServiceMock.Object,
        _cacheServiceMock.Object);

    private static Project BuildProjectWithMembers(long projectId, params long[] memberIds)
    {
        var project = ApplicationTestUtils.CreateInstanceWithoutConstructor<Project>();
        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.Id), projectId);
        var members = memberIds.Select(id =>
        {
            var member = ApplicationTestUtils.CreateInstanceWithoutConstructor<ProjectMember>();
            ApplicationTestUtils.SetPrivatePropertyValue(member, nameof(ProjectMember.UserId), id);
            return member;
        }).ToList();
        ApplicationTestUtils.SetPrivatePropertyValue(project, nameof(Project.ProjectMembers), members);
        return project;
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldCreateBillAndInvalidateCache()
    {
        const long userId = 10L;
        const long projectId = 42L;
        var command = new CreateBillCommand(projectId, "Internet", 50m, DateTime.UtcNow.Date.AddDays(5));

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _projectRepositoryMock
            .Setup(r => r.GetProjectWithMembersAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildProjectWithMembers(projectId, userId));

        var response = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.NotNull(response);
        Assert.True(response.Result.IsSuccess);

        _billRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Bill>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(CacheKeys.ProjectBills(projectId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(new CreateBillCommand(1L, "Title", 10m, DateTime.UtcNow.Date.AddDays(1)),
                CancellationToken.None));

        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ShouldThrowNotFoundException()
    {
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(1L);
        _projectRepositoryMock
            .Setup(r => r.GetProjectWithMembersAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().Handle(new CreateBillCommand(99L, "Title", 10m, DateTime.UtcNow.Date.AddDays(1)),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenUserNotMember_ShouldThrowAccessDeniedException()
    {
        const long projectId = 42L;
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(10L);

        _projectRepositoryMock
            .Setup(r => r.GetProjectWithMembersAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildProjectWithMembers(projectId, 999L));

        await Assert.ThrowsAsync<AccessDeniedException>(() =>
            CreateHandler().Handle(new CreateBillCommand(projectId, "Title", 10m, DateTime.UtcNow.Date.AddDays(1)),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithSplitUserIds_CreatesBillWithEqualSplit()
    {
        const long userId = 10L;
        const long otherUserId = 20L;
        const long projectId = 42L;

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _projectRepositoryMock
            .Setup(r => r.GetProjectWithMembersAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildProjectWithMembers(projectId, userId, otherUserId));

        var command = new CreateBillCommand(
            projectId, "Gym", 60m, DateTime.UtcNow.Date.AddDays(3),
            SplitUserIds: new[] { userId, otherUserId });

        var response = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(response.Result.IsSuccess);
        _billRepositoryMock.Verify(r => r.AddAsync(
            It.Is<Bill>(b => b.Splits.Count == 2),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithSplitForNonMember_ThrowsBusinessRuleValidationException()
    {
        const long userId = 10L;
        const long projectId = 42L;

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        _projectRepositoryMock
            .Setup(r => r.GetProjectWithMembersAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildProjectWithMembers(projectId, userId));

        var command = new CreateBillCommand(
            projectId, "Gym", 60m, DateTime.UtcNow.Date.AddDays(3),
            SplitUserIds: new[] { userId, 999L });

        await Assert.ThrowsAsync<BusinessRuleValidationException>(() =>
            CreateHandler().Handle(command, CancellationToken.None));
    }
}
