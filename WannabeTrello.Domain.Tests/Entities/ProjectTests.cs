using System.Reflection;
using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Enums;
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

    [Fact]
    public void Update_WithValidArguments_UpdatesPropertiesAndRaisesEvent()
    {
        // ARRANGE
        var project = CreateTestProject(
            1, 
            "Old Name", 
            "Old Description", 
            ProjectStatus.Active, 
            ProjectVisibility.Public, 
            false);

        const int updatedBy = 123;
        const string newName = "New Name";
        const string newDescription = "New Description";
        const ProjectStatus newStatus = ProjectStatus.Closed;
        const ProjectVisibility newVisibility = ProjectVisibility.Private;
        const bool newArchivedStatus = true;

        // ACT
        project.Update(newName, newDescription, newStatus, newVisibility, newArchivedStatus, updatedBy);

        // ASSERT
        Assert.Equal(newName, project.Name);
        Assert.Equal(newDescription, project.Description);
        Assert.Equal(newStatus, project.Status);
        Assert.Equal(newVisibility, project.Visibility);
        Assert.Equal(newArchivedStatus, project.IsArchived);

        // Provera da li su LastModifiedAt i LastModifiedBy ažurirani
        Assert.NotNull(project.LastModifiedAt);
        Assert.Equal(updatedBy, project.LastModifiedBy);

        // Provera da li je domenski događaj podignut
        Assert.Single(project.DomainEvents);
        var domainEvent = Assert.IsType<ProjectUpdatedEvent>(project.DomainEvents.First());
        Assert.Equal(project.Id, domainEvent.Id);
    }
    
    [Fact]
    public void Update_WithNoChanges_DoesNotUpdateAndDoesNotRaiseEvent()
    {
        

        var project = CreateTestProject(
            1,
            "Existing Name",
            "Existing Description",
            ProjectStatus.Active,
            ProjectVisibility.Public,
            false
        );

        var initialModifiedAt = project.LastModifiedAt;
        var initialModifiedBy = project.LastModifiedBy;
    
        // ACT
        project.Update("Existing Name", "Existing Description", ProjectStatus.Active, ProjectVisibility.Public, false, 123);

        // ASSERT
        Assert.Equal(initialModifiedAt, project.LastModifiedAt);
        Assert.Equal(initialModifiedBy, project.LastModifiedBy);
        
        Assert.Empty(project.DomainEvents);
    }
    
    [Fact]
    public void Update_WithPartialChanges_UpdatesOnlySpecifiedProperties()
    {
        
        var project = CreateTestProject(
            1,
            "Old Name",
            "Old Description",
            ProjectStatus.Active,
            ProjectVisibility.Public,
            false
        );

        var initialStatus = project.Status;
        const int updatedBy = 123;
    
        // ACT
        
        project.Update("New Name", null, null, ProjectVisibility.Private, false, updatedBy);

        // ASSERT
        Assert.Equal("New Name", project.Name);
        Assert.Equal("Old Description", project.Description);
        Assert.Equal(initialStatus, project.Status); 
        Assert.Equal(ProjectVisibility.Private, project.Visibility);
    
        Assert.NotNull(project.LastModifiedAt);
        Assert.Equal(updatedBy, project.LastModifiedBy);
        Assert.Single(project.DomainEvents);
    }


    private static Project CreateTestProject(long id, string name, string description, ProjectStatus status,
        ProjectVisibility visibility, bool isArchived)
    {
        var project = new Project();

        SetPrivatePropertyValue(project, "Id", id);
        SetPrivatePropertyValue(project, "Name", name);
        SetPrivatePropertyValue(project, "Description", description);
        SetPrivatePropertyValue(project, "Status", status);
        SetPrivatePropertyValue(project, "Visibility", visibility);
        SetPrivatePropertyValue(project, "IsArchived", isArchived);

        return project;
    }

    private static void SetPrivatePropertyValue<T>(T obj, string propertyName, object value)
    {
        typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(obj, value);
    }
}