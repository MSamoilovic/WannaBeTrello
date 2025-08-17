using WannabeTrello.Domain.Entities;

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
}