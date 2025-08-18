using System.Data;
using System.Reflection;
using Moq;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Application.Features.Projects.CreateProject;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Interfaces.Services;

namespace WannabeTrello.Application.Tests.Features.Projects;

public class CreateProjectCommandHandlerTests
{
    [Fact]
    public async Task Handle_AuthenticatedUser_ReturnsProjectId()
    {
        // ARRANGE (Priprema)
        
        // Mock-uj ICurrentUserService da simulira autentifikovanog korisnika
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(x => x.UserId).Returns(123);

        // Mock-uj ProjectService da simulira uspesno kreiranje projekta
        var projectServiceMock = new Mock<IProjectService>();
        
        var createdProject = new Project();
        SetPrivatePropertyValue(createdProject, nameof(createdProject.Id), 456);
        
        projectServiceMock
            .Setup(x => x.CreateProjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
            .ReturnsAsync(createdProject);

        // Kreiraj instancu handlera sa mock-ovima
        var handler = new CreateProjectCommandHandler(currentUserServiceMock.Object, projectServiceMock.Object);
        var command = new CreateProjectCommand("New Test Project", "Description");
        
        var projectId = await handler.Handle(command, CancellationToken.None);
        
        Assert.Equal(createdProject.Id, projectId);
        
        projectServiceMock.Verify(x => x.CreateProjectAsync(
            "New Test Project",
            "Description",
            123
        ), Times.Once);
    }

    [Fact]
    public async Task Handle_UnauthenticatedUser_ThrowsUnauthorizedAccessException()
    {
        
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(false);
        
        var projectServiceMock = new Mock<IProjectService>();
        
        var handler = new CreateProjectCommandHandler(currentUserServiceMock.Object, projectServiceMock.Object);
        var command = new CreateProjectCommand("Unauthorized Project", "Description");
        
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None)
        );
        
        projectServiceMock.Verify(x => x.CreateProjectAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<long>()
        ), Times.Never);
    }
    
    [Fact]
    public async Task Handle_ProjectServiceThrowsException_ThrowsException()
    {
        // ARRANGE (Priprema)
        
        // Mock-uj ICurrentUserService da simulira autentifikovanog korisnika
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        currentUserServiceMock.Setup(x => x.UserId).Returns(123);

        
        var projectServiceMock = new Mock<IProjectService>();
        projectServiceMock
            .Setup(x => x.CreateProjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
            .ThrowsAsync(new DataException("Database connection error.")); 

        // Kreiraj instancu handlera sa mock-ovima
        var handler = new CreateProjectCommandHandler(currentUserServiceMock.Object, projectServiceMock.Object);
        var command = new CreateProjectCommand("Failing Project", "Description");
        
        
        await Assert.ThrowsAsync<DataException>(
            () => handler.Handle(command, CancellationToken.None)
        );
        
        projectServiceMock.Verify(x => x.CreateProjectAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<long>()
        ), Times.Once);
    }
    
    private static void SetPrivatePropertyValue<T>(T obj, string propertyName, object value)
    {
        typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(obj, value);
    }
}