using Moq;
using WannabeTrello.Application.Common.Caching;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Labels.DeleteLabel;
using WannabeTrello.Application.Tests.Utils;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces;
using WannabeTrello.Domain.Interfaces.Repositories;

namespace WannabeTrello.Application.Tests.Features.Labels;

public class DeleteLabelCommandHandlerTests
{
    private readonly Mock<ILabelRepository> _labelRepositoryMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();

    private DeleteLabelCommandHandler CreateHandler() => new(
        _labelRepositoryMock.Object,
        _currentUserServiceMock.Object,
        _unitOfWorkMock.Object,
        _cacheServiceMock.Object);

    [Fact]
    public async Task Handle_WhenLabelExists_ShouldDeleteAndInvalidateCache()
    {
        // Arrange
        var userId = 1L;
        var labelId = 5L;
        var boardId = 10L;

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var label = Label.Create("Bug", "#FF0000", boardId, userId);
        ApplicationTestUtils.SetPrivatePropertyValue(label, nameof(Label.Id), labelId);

        _labelRepositoryMock.Setup(r => r.GetByIdAsync(labelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(label);

        // Act
        var response = await CreateHandler().Handle(new DeleteLabelCommand(labelId), CancellationToken.None);

        // Assert
        Assert.True(response.Success);
        _labelRepositoryMock.Verify(r => r.Delete(label), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(CacheKeys.BoardLabels(boardId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(new DeleteLabelCommand(1L), CancellationToken.None));

        _labelRepositoryMock.Verify(r => r.Delete(It.IsAny<Label>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenLabelNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(1L);
        _labelRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Label?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            CreateHandler().Handle(new DeleteLabelCommand(99L), CancellationToken.None));

        _labelRepositoryMock.Verify(r => r.Delete(It.IsAny<Label>()), Times.Never);
    }
}
