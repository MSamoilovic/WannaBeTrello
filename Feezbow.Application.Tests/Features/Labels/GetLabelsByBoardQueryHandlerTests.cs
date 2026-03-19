using Moq;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Labels.GetLabelsByBoard;
using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Application.Tests.Features.Labels;

public class GetLabelsByBoardQueryHandlerTests
{
    private readonly Mock<ILabelRepository> _labelRepositoryMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();

    private GetLabelsByBoardQueryHandler CreateHandler() => new(
        _labelRepositoryMock.Object,
        _currentUserServiceMock.Object,
        _cacheServiceMock.Object);

    [Fact]
    public async Task Handle_WhenBoardHasLabels_ShouldReturnMappedLabels()
    {
        // Arrange
        var userId = 1L;
        var boardId = 10L;

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

        var labels = new List<Label>
        {
            Label.Create("Bug", "#FF0000", boardId, userId),
            Label.Create("Feature", "#00FF00", boardId, userId),
        };

        _cacheServiceMock.Setup(c => c.GetOrSetAsync(
                CacheKeys.BoardLabels(boardId),
                It.IsAny<Func<Task<IReadOnlyList<Label>>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(labels.AsReadOnly());

        // Act
        var response = await CreateHandler().Handle(new GetLabelsByBoardQuery(boardId), CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Count);
        Assert.Contains(response, r => r.Name == "Bug" && r.Color == "#FF0000");
        Assert.Contains(response, r => r.Name == "Feature" && r.Color == "#00FF00");
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        _currentUserServiceMock.Setup(s => s.UserId).Returns((long?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            CreateHandler().Handle(new GetLabelsByBoardQuery(1L), CancellationToken.None));

        _cacheServiceMock.Verify(c => c.GetOrSetAsync<IReadOnlyList<Label>>(
            It.IsAny<string>(), It.IsAny<Func<Task<IReadOnlyList<Label>>>>(),
            It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenBoardHasNoLabels_ShouldReturnEmptyList()
    {
        // Arrange
        var boardId = 10L;
        _currentUserServiceMock.Setup(s => s.UserId).Returns(1L);

        _cacheServiceMock.Setup(c => c.GetOrSetAsync(
                CacheKeys.BoardLabels(boardId),
                It.IsAny<Func<Task<IReadOnlyList<Label>>>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Label>().AsReadOnly());

        // Act
        var response = await CreateHandler().Handle(new GetLabelsByBoardQuery(boardId), CancellationToken.None);

        // Assert
        Assert.Empty(response);
    }
}
