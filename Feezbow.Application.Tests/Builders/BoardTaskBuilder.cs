using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;
using Feezbow.Domain.ValueObjects;

namespace Feezbow.Application.Tests.Builders;

public class BoardTaskBuilder
{
    private long _id = 1;
    private string _title = "Test Task";
    private string? _description = null;
    private int _position = 0;
    private TaskPriority _priority = TaskPriority.Medium;
    private DateTime? _dueDate = null;
    private long _columnId = 1;
    private long? _assigneeId = null;
    private bool _isArchived = false;
    private long _createdBy = 1;

    public BoardTaskBuilder WithId(long id)
    {
        _id = id;
        return this;
    }

    public BoardTaskBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public BoardTaskBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public BoardTaskBuilder WithPosition(int position)
    {
        _position = position;
        return this;
    }

    public BoardTaskBuilder WithPriority(TaskPriority priority)
    {
        _priority = priority;
        return this;
    }

    public BoardTaskBuilder WithDueDate(DateTime? dueDate)
    {
        _dueDate = dueDate;
        return this;
    }

    public BoardTaskBuilder WithColumnId(long columnId)
    {
        _columnId = columnId;
        return this;
    }

    public BoardTaskBuilder WithAssigneeId(long? assigneeId)
    {
        _assigneeId = assigneeId;
        return this;
    }

    public BoardTaskBuilder Archived()
    {
        _isArchived = true;
        return this;
    }

    public BoardTaskBuilder WithCreatedBy(long userId)
    {
        _createdBy = userId;
        return this;
    }

    public BoardTask Build()
    {
        var task = ApplicationTestUtils.CreateInstanceWithoutConstructor<BoardTask>();
        ApplicationTestUtils.InitializeDomainEvents(task);
        ApplicationTestUtils.SetPrivatePropertyValue(task, "Id", _id);
        ApplicationTestUtils.SetPrivatePropertyValue(task, "Title", _title);
        ApplicationTestUtils.SetPrivatePropertyValue(task, "Description", _description);
        ApplicationTestUtils.SetPrivatePropertyValue(task, "Position", _position);
        ApplicationTestUtils.SetPrivatePropertyValue(task, "Priority", _priority);
        ApplicationTestUtils.SetPrivatePropertyValue(task, "DueDate", _dueDate);
        ApplicationTestUtils.SetPrivatePropertyValue(task, "ColumnId", _columnId);
        ApplicationTestUtils.SetPrivatePropertyValue(task, "AssigneeId", _assigneeId);
        ApplicationTestUtils.SetPrivatePropertyValue(task, "IsArchived", _isArchived);
        ApplicationTestUtils.SetPrivatePropertyValue(task, "CreatedBy", _createdBy);
        ApplicationTestUtils.SetPrivatePropertyValue(task, "_taskLabels", new List<BoardTaskLabel>());
        ApplicationTestUtils.SetPrivatePropertyValue(task, "_activities", new List<Activity>());
        ApplicationTestUtils.SetPrivatePropertyValue(task, "Comments", new List<Comment>());
        return task;
    }
}
