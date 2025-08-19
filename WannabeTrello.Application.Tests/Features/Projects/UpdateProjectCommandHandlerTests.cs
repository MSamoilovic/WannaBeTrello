using System.Data;
using System.Reflection;
using Moq;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Projects.UpdateProject;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.Projects;

public class UpdateProjectCommandHandlerTests
{
    [Fact]
    public async Task Handle_AuthenticatedUser_ReturnsCorrectResponse()
    {
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(x => x.UserId).Returns(123);

        var projectServiceMock = new Mock<IProjectService>();

        var updatedProject = new Project();
        SetPrivatePropertyValue(updatedProject, "Id", 1);
        SetPrivatePropertyValue(updatedProject, "Name", "Updated Name");
        SetPrivatePropertyValue(updatedProject, "Description", "Updated Description");
        SetPrivatePropertyValue(updatedProject, "Visibility", ProjectVisibility.Public);
        SetPrivatePropertyValue(updatedProject, "Status", ProjectStatus.Active);
        SetPrivatePropertyValue(updatedProject, "IsArchived", false);

        projectServiceMock.Setup(x => x.UpdateProjectAsync(
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ProjectStatus>(),
                It.IsAny<ProjectVisibility>(),
                It.IsAny<bool>(),
                It.IsAny<long>()))
            .ReturnsAsync(updatedProject);

        var handler = new UpdateProjectCommandHandler(projectServiceMock.Object, currentUserServiceMock.Object);
        var command = new UpdateProjectCommand(1, "Updated Name", "Updated Description",
            ProjectStatus.Active, ProjectVisibility.Public, false);

        var response = await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal(updatedProject.Name, response.Name);
        Assert.Equal(updatedProject.Description, response.Description);
        Assert.Equal(updatedProject.Visibility, response.Visibility);
        Assert.Equal(updatedProject.Status, response.Status);
        Assert.Equal(updatedProject.IsArchived, response.IsArchived);

        projectServiceMock.Verify(x => x.UpdateProjectAsync(
            command.ProjectId,
            command.Name,
            command.Description,
            command.Status,
            command.Visibility,
            command.Archived,
            123
        ), Times.Once);
    }

    [Theory]
    [InlineData(false, null)]
    [InlineData(true, null)]
    public async Task Handle_UnauthorizedUser_ThrowsUnauthorizedAccessException(bool isAuthenticated, long? userId)
    {
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(isAuthenticated);
        currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var projectServiceMock = new Mock<IProjectService>();

        var handler = new UpdateProjectCommandHandler(projectServiceMock.Object, currentUserServiceMock.Object);
        var command =
            new UpdateProjectCommand(1, "Name", "Desc", ProjectStatus.Active, ProjectVisibility.Public, false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));

        projectServiceMock.Verify(x => x.UpdateProjectAsync(
            It.IsAny<long>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<ProjectStatus>(),
            It.IsAny<ProjectVisibility>(),
            It.IsAny<bool>(),
            It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ProjectServiceThrowsException_ThrowsException()
    {
        // ARRANGE
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(x => x.UserId).Returns(123);

        var projectServiceMock = new Mock<IProjectService>();
        projectServiceMock
            .Setup(x => x.UpdateProjectAsync(
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ProjectStatus>(),
                It.IsAny<ProjectVisibility>(),
                It.IsAny<bool>(),
                It.IsAny<long>()))
            .ThrowsAsync(new DataException("Database connection error."));

        var handler = new UpdateProjectCommandHandler(projectServiceMock.Object, currentUserServiceMock.Object);
        var command =
            new UpdateProjectCommand(1, "Name", "Desc", ProjectStatus.Active, ProjectVisibility.Public, false);

        // ACT & ASSERT
        await Assert.ThrowsAsync<DataException>(() => handler.Handle(command, CancellationToken.None));

        projectServiceMock.Verify(x => x.UpdateProjectAsync(
            It.IsAny<long>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<ProjectStatus>(),
            It.IsAny<ProjectVisibility>(),
            It.IsAny<bool>(),
            It.IsAny<long>()), Times.Once);
    }
    
    
    private static void SetPrivatePropertyValue<T>(T obj, string propertyName, object value)
    {
        typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(obj, value);
    }
}