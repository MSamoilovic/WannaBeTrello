using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;
using Feezbow.Domain.ValueObjects;

namespace Feezbow.Application.Tests.Builders;

public class ProjectBuilder
{
    private long _id = 1;
    private string _name = "Test Project";
    private string? _description = null;
    private long _ownerId = 1;
    private ProjectStatus _status = ProjectStatus.Active;
    private ProjectVisibility _visibility = ProjectVisibility.Public;
    private bool _isArchived = false;

    public ProjectBuilder WithId(long id)
    {
        _id = id;
        return this;
    }

    public ProjectBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ProjectBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public ProjectBuilder WithOwnerId(long ownerId)
    {
        _ownerId = ownerId;
        return this;
    }

    public ProjectBuilder WithStatus(ProjectStatus status)
    {
        _status = status;
        return this;
    }

    public ProjectBuilder WithVisibility(ProjectVisibility visibility)
    {
        _visibility = visibility;
        return this;
    }

    public ProjectBuilder Archived()
    {
        _isArchived = true;
        return this;
    }

    public Project Build()
    {
        var project = ApplicationTestUtils.CreateInstanceWithoutConstructor<Project>();
        ApplicationTestUtils.InitializeDomainEvents(project);
        ApplicationTestUtils.SetPrivatePropertyValue(project, "Id", _id);
        ApplicationTestUtils.SetPrivatePropertyValue(project, "Name", _name);
        ApplicationTestUtils.SetPrivatePropertyValue(project, "Description", _description);
        ApplicationTestUtils.SetPrivatePropertyValue(project, "OwnerId", _ownerId);
        ApplicationTestUtils.SetPrivatePropertyValue(project, "Status", _status);
        ApplicationTestUtils.SetPrivatePropertyValue(project, "Visibility", _visibility);
        ApplicationTestUtils.SetPrivatePropertyValue(project, "IsArchived", _isArchived);
        ApplicationTestUtils.SetPrivatePropertyValue(project, "_activities", new List<Activity>());
        ApplicationTestUtils.SetPrivatePropertyValue(project, "Boards", new List<Board>());
        ApplicationTestUtils.SetPrivatePropertyValue(project, "ProjectMembers", new List<ProjectMember>());
        return project;
    }
}
