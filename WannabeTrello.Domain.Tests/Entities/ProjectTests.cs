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
        project.Update("Existing Name", "Existing Description", ProjectStatus.Active, ProjectVisibility.Public, false,
            123);

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

    [Fact]
    public void Archive_UserIsAdmin_ArchivesProjectAndRaisesEvent()
    {
        const int projectId = 1;
        const int adminUserId = 123;

        var project = CreateTestProject(
            projectId,
            "Project Name",
            "Project Description",
            ProjectStatus.Active,
            ProjectVisibility.Public,
            false
        );

        var members = CreateTestProjectMember(adminUserId, projectId, ProjectRole.Owner);
        project.ProjectMembers.Add(members[0]);

        // ACT
        project.Archive(adminUserId);

        Assert.True(project.IsArchived);

        Assert.NotNull(project.LastModifiedAt);
        Assert.Equal(adminUserId, project.LastModifiedBy);

        Assert.Single(project.DomainEvents);
        var domainEvent = Assert.IsType<ProjectArchivedEvent>(project.DomainEvents.First());
        Assert.Equal(projectId, domainEvent.ProjectId);
    }

    [Theory]
    [InlineData(ProjectRole.Contributor, "Only Owner or Admin can archive this project")]
    public void Archive_UnauthorizedUser_ThrowsUnauthorizedAccessException(
        ProjectRole unauthorizedRole, string expectedErrorMessage)
    {
        const int projectId = 1;
        const int unauthorizedUserId = 999;
        const int adminUserId = 123;

        var project = CreateTestProject(
            projectId,
            "Project Name",
            "Project Description",
            ProjectStatus.Active,
            ProjectVisibility.Public,
            false
        );

        var members = CreateTestProjectMember(adminUserId, projectId, unauthorizedRole);
        project.ProjectMembers.Add(members[0]);

        // ACT & ASSERT
        var ex = Assert.Throws<UnauthorizedAccessException>(() => project.Archive(unauthorizedUserId));
        Assert.Equal(expectedErrorMessage, ex.Message);

        Assert.False(project.IsArchived);
        Assert.Empty(project.DomainEvents);
    }
    
    

    [Theory]
    [InlineData(ProjectStatus.Closed)]
    [InlineData(ProjectStatus.OnHold)]
    public void Archive_NonActiveProject_ThrowsInvalidOperationException(ProjectStatus status)
    {
        // ARRANGE
        const int ownerUserId = 123;
        
        var project = CreateTestProject(
            1,
            "Project Name",
            "Project Description",
            status,
            ProjectVisibility.Public,
            false
        );

        var members = CreateTestProjectMember(ownerUserId, 1, ProjectRole.Owner);
        project.ProjectMembers.Add(members[0]);


        // ACT & ASSERT
        Assert.Throws<InvalidOperationException>(() => project.Archive(ownerUserId));
        Assert.False(project.IsArchived);
        Assert.Empty(project.DomainEvents);
    }
    
    [Fact]
    public void Archive_AlreadyArchivedProject_DoesNothing()
    {
        // ARRANGE
        const int ownerUserId = 123;
        var project = CreateTestProject(
            1,
            "Project Name",
            "Project Description",
            ProjectStatus.Archived,
            ProjectVisibility.Public,
            true
        );

        var members = CreateTestProjectMember(ownerUserId, 1, ProjectRole.Owner);
        project.ProjectMembers.Add(members[0]);

        var initialModifiedAt = project.LastModifiedAt;

        
        project.Archive(ownerUserId);

        
        Assert.Equal(initialModifiedAt, project.LastModifiedAt);
        
        Assert.Empty(project.DomainEvents);
    }
    
    //Tests for Adding Project Member
    [Fact]
    public void AddMember_OwnerAddsNewMember_AddsMemberAndRaisesEvent()
    {
        // ARRANGE
        const long ownerId = 1;
        const long newMemberId = 2;
        var projectId = 1;

        // Kreiramo projekat bez članova
        var project = CreateTestProject(projectId, "Test Project", "Desc", ProjectStatus.Active, ProjectVisibility.Public, false);
        
        // Ručno dodajemo vlasnika projekta
        var ownerMembers = CreateTestProjectMember(ownerId, projectId, ProjectRole.Owner);
        SetPrivatePropertyValue(project, "ProjectMembers", ownerMembers);
        
        // ACT
        project.AddMember(newMemberId, ProjectRole.Contributor, ownerId);

        // ASSERT
        var newMember = project.ProjectMembers.FirstOrDefault(m => m.UserId == newMemberId);
        Assert.NotNull(newMember);
        Assert.Equal(ProjectRole.Contributor, newMember.Role);
        Assert.Single(project.DomainEvents.OfType<ProjectMemberAddedEvent>());
    }

    [Fact]
    public void AddMember_AdminAddsNewMember_AddsMemberAndRaisesEvent()
    {
        // ARRANGE
        const long ownerId = 1;
        const long adminId = 2;
        const long newMemberId = 3;
        var projectId = 1;

        var project = CreateTestProject(projectId, "Test Project", "Desc", ProjectStatus.Active, ProjectVisibility.Public, false);
        
        // Ručno dodajemo vlasnika i admina
        var members = CreateTestProjectMember(ownerId, projectId, ProjectRole.Owner);
        members.AddRange(CreateTestProjectMember(adminId, projectId, ProjectRole.Admin));
        SetPrivatePropertyValue(project, "ProjectMembers", members);
        
        // ACT
        project.AddMember(newMemberId, ProjectRole.Contributor, adminId);

        // ASSERT
        var newMember = project.ProjectMembers.FirstOrDefault(m => m.UserId == newMemberId);
        Assert.NotNull(newMember);
        Assert.Equal(ProjectRole.Contributor, newMember.Role);
        Assert.Single(project.DomainEvents.OfType<ProjectMemberAddedEvent>());
    }

    [Fact]
    public void AddMember_UserIsAlreadyMember_ThrowsUnauthorizedAccessException()
    {
        // ARRANGE
        const long ownerId = 1;
        const int projectId = 1;
        var project = CreateTestProject(projectId, "Test Project", "Desc", ProjectStatus.Active, ProjectVisibility.Public, false);
        var members = CreateTestProjectMember(ownerId, projectId, ProjectRole.Owner);
        SetPrivatePropertyValue(project, "ProjectMembers", members);

        // ACT & ASSERT
        Assert.Throws<UnauthorizedAccessException>(() => 
            project.AddMember(ownerId, ProjectRole.Contributor, ownerId));
    }


    [Fact]
    public void AddMember_NonAdminOrOwnerAddsNewMember_ThrowsUnauthorizedAccessException()
    {
        // ARRANGE
        const long ownerId = 1;
        const long nonAdminMemberId = 2;
        const long newMemberId = 3;
        var projectId = 1;

        // Kreiramo projekat bez članova
        var project = CreateTestProject(projectId, "Test Project", "Desc", ProjectStatus.Active, ProjectVisibility.Public, false);
    
        // Ručno dodajemo vlasnika i korisnika sa ulogom Contributor
        var members = CreateTestProjectMember(ownerId, projectId, ProjectRole.Owner);
        members.AddRange(CreateTestProjectMember(nonAdminMemberId, projectId, ProjectRole.Contributor));
        SetPrivatePropertyValue(project, "ProjectMembers", members);
    
        // ACT & ASSERT
        // Pokušavamo da dodamo novog člana sa ID-jem korisnika koji nije ni Owner ni Admin
        Assert.Throws<UnauthorizedAccessException>(() => 
            project.AddMember(newMemberId, ProjectRole.Contributor, nonAdminMemberId));
        
        Assert.DoesNotContain(project.ProjectMembers, m => m.UserId == newMemberId);
        
        Assert.Empty(project.DomainEvents);
    }
    
     // --- TESTOVI ZA REMOVEMEMBER ---

    [Fact]
    public void RemoveMember_OwnerRemovesMember_RemovesMemberAndRaisesEvent()
    {
        // ARRANGE
        const long ownerId = 1;
        const long memberIdToRemove = 2;
        var projectId = 1;
        
        // Kreiramo projekat sa vlasnikom i dodatnim članom
        var project = CreateTestProject(projectId, "Test Project", "Desc", ProjectStatus.Active, ProjectVisibility.Public, false);
        var members = CreateTestProjectMember(ownerId, projectId, ProjectRole.Owner);
        members.AddRange(CreateTestProjectMember(memberIdToRemove, projectId, ProjectRole.Contributor));
        SetPrivatePropertyValue(project, "ProjectMembers", members);

        // ACT
        project.RemoveMember(memberIdToRemove, ownerId);

        // ASSERT
        // Proveravamo da član više nije u projektu
        Assert.DoesNotContain(project.ProjectMembers, m => m.UserId == memberIdToRemove);
        // Proveravamo da je događaj pokrenut
        var domainEvent = project.DomainEvents.OfType<ProjectMemberRemovedEvent>().FirstOrDefault();
        Assert.NotNull(domainEvent);
        Assert.Equal(memberIdToRemove, domainEvent.RemovedUserId);
        Assert.Equal(ownerId, domainEvent.RemovingUserId);
    }
    
    [Fact]
    public void RemoveMember_AdminRemovesMember_RemovesMemberAndRaisesEvent()
    {
        // ARRANGE
        const long ownerId = 1;
        const long adminId = 2;
        const long memberIdToRemove = 3;
        var projectId = 1;
        
        // Kreiramo projekat sa vlasnikom, adminom i članom koji se uklanja
        var project = CreateTestProject(projectId, "Test Project", "Desc", ProjectStatus.Active, ProjectVisibility.Public, false);
        var members = CreateTestProjectMember(ownerId, projectId, ProjectRole.Owner);
        members.AddRange(CreateTestProjectMember(adminId, projectId, ProjectRole.Admin));
        members.AddRange(CreateTestProjectMember(memberIdToRemove, projectId, ProjectRole.Contributor));
        SetPrivatePropertyValue(project, "ProjectMembers", members);

        // ACT
        project.RemoveMember(memberIdToRemove, adminId);

        // ASSERT
        Assert.DoesNotContain(project.ProjectMembers, m => m.UserId == memberIdToRemove);
        var domainEvent = project.DomainEvents.OfType<ProjectMemberRemovedEvent>().FirstOrDefault();
        Assert.NotNull(domainEvent);
        Assert.Equal(memberIdToRemove, domainEvent.RemovedUserId);
        Assert.Equal(adminId, domainEvent.RemovingUserId);
    }
    
    [Fact]
    public void RemoveMember_NonAdminOrOwnerRemovesMember_ThrowsUnauthorizedAccessException()
    {
        // ARRANGE
        const long ownerId = 1;
        const long nonAdminMemberId = 2;
        const long memberIdToRemove = 3;
        var projectId = 1;
        
        // Kreiramo projekat sa vlasnikom, članom sa ulogom Contributor i članom za uklanjanje
        var project = CreateTestProject(projectId, "Test Project", "Desc", ProjectStatus.Active, ProjectVisibility.Public, false);
        var members = CreateTestProjectMember(ownerId, projectId, ProjectRole.Owner);
        members.AddRange(CreateTestProjectMember(nonAdminMemberId, projectId, ProjectRole.Contributor));
        members.AddRange(CreateTestProjectMember(memberIdToRemove, projectId, ProjectRole.Contributor));
        SetPrivatePropertyValue(project, "ProjectMembers", members);

        // ACT & ASSERT
        Assert.Throws<UnauthorizedAccessException>(() => 
            project.RemoveMember(memberIdToRemove, nonAdminMemberId));
        
        // Proveravamo da član nije uklonjen
        Assert.Contains(project.ProjectMembers, m => m.UserId == memberIdToRemove);
    }
    
    [Fact]
    public void RemoveMember_MemberDoesNotExist_DoesNothingAndDoesNotRaiseEvent()
    {
        // ARRANGE
        const long ownerId = 1;
        const long nonExistentMemberId = 99;
        var projectId = 1;
        var project = CreateTestProject(projectId, "Test Project", "Desc", ProjectStatus.Active, ProjectVisibility.Public, false);
        var members = CreateTestProjectMember(ownerId, projectId, ProjectRole.Owner);
        SetPrivatePropertyValue(project, "ProjectMembers", members);
        
        // Zapamćujemo početni broj članova i događaja
        var initialMemberCount = project.ProjectMembers.Count;
        var initialDomainEventsCount = project.DomainEvents.Count;

        // ACT
        project.RemoveMember(nonExistentMemberId, ownerId);

        // ASSERT
        // Proveravamo da broj članova ostaje isti
        Assert.Equal(initialMemberCount, project.ProjectMembers.Count);
        // Proveravamo da nije pokrenut nikakav događaj
        Assert.Equal(initialDomainEventsCount, project.DomainEvents.Count);
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
    
    // --- TESTOVI ZA UPDATEMEMBER ---

    [Fact]
    public void UpdateMember_OwnerUpdatesMemberRole_UpdatesRoleAndRaisesEvent()
    {
        // ARRANGE
        const long ownerId = 1;
        const long memberToUpdateId = 2;
        var projectId = 1;

        // Kreiramo projekat sa vlasnikom i članom
        var project = CreateTestProject(projectId, "Test Project", "Desc", ProjectStatus.Active, ProjectVisibility.Public, false);
        var members = CreateTestProjectMember(ownerId, projectId, ProjectRole.Owner);
        members.AddRange(CreateTestProjectMember(memberToUpdateId, projectId, ProjectRole.Contributor));
        SetPrivatePropertyValue(project, "ProjectMembers", members);

        // ACT
        project.UpdateMember(memberToUpdateId, ProjectRole.Admin, ownerId);

        // ASSERT
        var updatedMember = project.ProjectMembers.FirstOrDefault(m => m.UserId == memberToUpdateId);
        Assert.NotNull(updatedMember);
        Assert.Equal(ProjectRole.Admin, updatedMember.Role);
        
        var domainEvent = project.DomainEvents.OfType<ProjectMemberUpdatedEvent>().FirstOrDefault();
        Assert.NotNull(domainEvent);
        Assert.Equal(memberToUpdateId, domainEvent.UpdatedMemberId);
        Assert.Equal(ProjectRole.Admin, domainEvent.Role);
    }
    
    [Fact]
    public void UpdateMember_AdminUpdatesMemberRole_UpdatesRoleAndRaisesEvent()
    {
        // ARRANGE
        const long ownerId = 1;
        const long adminId = 2;
        const long memberToUpdateId = 3;
        var projectId = 1;

        // Kreiramo projekat sa vlasnikom, adminom i članom za ažuriranje
        var project = CreateTestProject(projectId, "Test Project", "Desc", ProjectStatus.Active, ProjectVisibility.Public, false);
        var members = CreateTestProjectMember(ownerId, projectId, ProjectRole.Owner);
        members.AddRange(CreateTestProjectMember(adminId, projectId, ProjectRole.Admin));
        members.AddRange(CreateTestProjectMember(memberToUpdateId, projectId, ProjectRole.Contributor));
        SetPrivatePropertyValue(project, "ProjectMembers", members);

        // ACT
        project.UpdateMember(memberToUpdateId, ProjectRole.Contributor, adminId);

        // ASSERT
        var updatedMember = project.ProjectMembers.FirstOrDefault(m => m.UserId == memberToUpdateId);
        Assert.NotNull(updatedMember);
        Assert.Equal(ProjectRole.Contributor, updatedMember.Role);

        var domainEvent = project.DomainEvents.OfType<ProjectMemberUpdatedEvent>().FirstOrDefault();
        Assert.NotNull(domainEvent);
        Assert.Equal(memberToUpdateId, domainEvent.UpdatedMemberId);
        Assert.Equal(ProjectRole.Contributor, domainEvent.Role);
    }
    
    [Fact]
    public void UpdateMember_NonAdminOrOwnerUpdatesMemberRole_ThrowsUnauthorizedAccessException()
    {
        // ARRANGE
        const long ownerId = 1;
        const long nonAdminMemberId = 2;
        const long memberToUpdateId = 3;
        var projectId = 1;
        var project = CreateTestProject(projectId, "Test Project", "Desc", ProjectStatus.Active, ProjectVisibility.Public, false);
        var members = CreateTestProjectMember(ownerId, projectId, ProjectRole.Owner);
        members.AddRange(CreateTestProjectMember(nonAdminMemberId, projectId, ProjectRole.Contributor));
        members.AddRange(CreateTestProjectMember(memberToUpdateId, projectId, ProjectRole.Contributor));
        SetPrivatePropertyValue(project, "ProjectMembers", members);

        // ACT & ASSERT
        Assert.Throws<UnauthorizedAccessException>(() =>
            project.UpdateMember(memberToUpdateId, ProjectRole.Admin, nonAdminMemberId));
        
        var member = project.ProjectMembers.FirstOrDefault(m => m.UserId == memberToUpdateId);
        Assert.Equal(ProjectRole.Contributor, member?.Role);
    }

    [Fact]
    public void UpdateMember_MemberDoesNotExist_DoesNothing()
    {
        // ARRANGE
        const long ownerId = 1;
        const long nonExistentMemberId = 99;
        var projectId = 1;
        var project = CreateTestProject(projectId, "Test Project", "Desc", ProjectStatus.Active, ProjectVisibility.Public, false);
        var members = CreateTestProjectMember(ownerId, projectId, ProjectRole.Owner);
        SetPrivatePropertyValue(project, "ProjectMembers", members);
        
        var initialModifiedAt = project.LastModifiedAt;

        // ACT
        project.UpdateMember(nonExistentMemberId, ProjectRole.Admin, ownerId);

        // ASSERT
        Assert.Equal(initialModifiedAt, project.LastModifiedAt);
        Assert.Empty(project.DomainEvents);
    }
    
    [Fact]
    public void UpdateMember_OwnerTriesToUpdateTheirOwnRole_ThrowsInvalidOperationException()
    {
        // ARRANGE
        const long ownerId = 1;
        var projectId = 1;
        var project = CreateTestProject(projectId, "Test Project", "Desc", ProjectStatus.Active, ProjectVisibility.Public, false);
        var members = CreateTestProjectMember(ownerId, projectId, ProjectRole.Owner);
        SetPrivatePropertyValue(project, "ProjectMembers", members);
        
        // ACT & ASSERT
        Assert.Throws<InvalidOperationException>(() =>
            project.UpdateMember(ownerId, ProjectRole.Contributor, ownerId));
    }
    

    private static List<ProjectMember> CreateTestProjectMember(long memberId, long projectId, ProjectRole role)
    {
        var members = new List<ProjectMember>();
        var member = ProjectMember.Create(memberId, projectId, role);
        members.Add(member);

        return members;
    }

    private static void SetPrivatePropertyValue<T>(T obj, string propertyName, object value)
    {
        typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(obj, value);
    }
}