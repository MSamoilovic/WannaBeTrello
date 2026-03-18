using Moq;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Labels.CreateLabel;
using WannabeTrello.Application.Tests.Utils;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Application.Tests.Features.Labels;

public class CreateLabelCommandHandlerTests
{
    private readonly Mock<ILabelRepository> _labelRepositoryMock = new();
    private readonly Mock<IBoardRepository> _boardRepositoryMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();

    private CreateLabelCommandHandler CreateHandler() => new(
        _labelRepositoryMock.Object,
        _boardRepositoryMock.Object,
        _currentUserServiceMock.Object,
        _unitOfWorkMock.Object,
        _cacheServiceMock.Object);

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldCreateLabelAndInvalidateCache()
    {
        // Arrange
        var userId = 1L;
        var boardId = 10L;
        var command = new CreateLabelCommand(boardId, "Bug", "#FF0000");

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var board = ApplicationTestUtils.CreateInstanceWithoutConstructor<Board>();
        var boardMember = ApplicationTestUtils.CreateInstanceWithoutConstructor<BoardMember>();
        ApplicationTestUtils.SetPrivatePropertyValue(boardMember, nameof(BoardMember.UserId), userId);
        ApplicationTestUtils.SetPrivatePropertyValue(board, nameof(Board.BoardMembers), new List<BoardMember> { boardMember });

        _boardRepositoryMock.Setup(r => r.GetBoardWithDetailsAsync(boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(board);

        _labelRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Label>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var response = await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Bug", response.Name);
        Assert.Equal("#FF0000", response.Color);
        Assert.Equal(boardId, response.BoardId);

        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(CacheKeys.BoardLabels(boardId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);
        var command = new CreateLabelCommand(1L, "Bug", "#FF0000");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(command, CancellationToken.None));

        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenBoardNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(1L);
        _boardRepositoryMock.Setup(r => r.GetBoardWithDetailsAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Board?)null);

        var command = new CreateLabelCommand(99L, "Bug", "#FF0000");

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            CreateHandler().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenUserIsNotBoardMember_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var userId = 1L;
        var boardId = 10L;

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var board = ApplicationTestUtils.CreateInstanceWithoutConstructor<Board>();
        ApplicationTestUtils.SetPrivatePropertyValue(board, nameof(Board.BoardMembers), new List<BoardMember>());

        _boardRepositoryMock.Setup(r => r.GetBoardWithDetailsAsync(boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(board);

        var command = new CreateLabelCommand(boardId, "Bug", "#FF0000");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(command, CancellationToken.None));

        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
