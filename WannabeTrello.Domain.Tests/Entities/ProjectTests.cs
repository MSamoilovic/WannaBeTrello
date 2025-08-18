using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Events.Project_Events;

namespace WannabeTrello.Domain.Tests.Entities;

public class ProjectTests
{
    //Create Project Test Methods
    [Fact]
    [Trait("Category", "Domain")]
    public void Create_ValidArguments_ReturnsCorrectlyInitializedProject()
    {
        
        const string projectName = "Test Project";
        const string description = "This is a test project description.";
        const int creatorUserId = 123;
        
        var project = Project.Create(projectName, description, creatorUserId);
        
        Assert.NotNull(project);
        Assert.Equal(projectName, project.Name);
        Assert.Equal(description, project.Description);
        Assert.Equal(creatorUserId, project.CreatedBy);
        Assert.True(project.CreatedAt <= DateTime.UtcNow);
        
        Assert.Single(project.DomainEvents);
        var domainEvent = Assert.IsType<ProjectCreatedEvent>(project.DomainEvents.First());
        Assert.Equal(project.Id, domainEvent.ProjectId);
        Assert.Equal(projectName, domainEvent.ProjectName);
        Assert.Equal(creatorUserId, domainEvent.OwnerId);
    }
    
    [Fact]
    [Trait("Category", "Domain")]
    public void Create_NullOrWhitespaceName_ThrowsArgumentException()
    {
       
        const string description = "This project should fail.";
        const int creatorUserId = 123;
        
        Assert.Throws<ArgumentException>(() => Project.Create(null, description, creatorUserId));
        Assert.Throws<ArgumentException>(() => Project.Create(string.Empty, description, creatorUserId));
        Assert.Throws<ArgumentException>(() => Project.Create("   ", description, creatorUserId));
    }
    
    [Fact]
    [Trait("Category", "Domain")]
    public void Create_InvalidCreatorId_ThrowsArgumentException()
    {
        
        const string projectName = "Project with Invalid Creator";
        const string description = "This project should fail.";
        const int creatorUserId = 0; 
        
        Assert.Throws<ArgumentException>(() => Project.Create(projectName, description, creatorUserId));
    }
    
    [Theory]
    [InlineData("New Project Name", null)]
    [InlineData("Another Project", "")]
    [InlineData("Third Project", "   ")]
    [InlineData("Fourth Project", "This is a description")]
    public void Create_WithVariousDescriptionInputs_SetsDescriptionCorrectly(string name, string description)
    {
        const int ownerId = 123;
        
        var project = Project.Create(name, description, ownerId);
        
        Assert.Equal(description, project.Description);
        
        Assert.Equal(name, project.Name);
        Assert.Single(project.ProjectMembers);
    }
}