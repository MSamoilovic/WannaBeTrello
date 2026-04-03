using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;

namespace Feezbow.Application.Tests.Builders;

public class BoardBuilder
{
    private long _id = 1;
    private string _name = "Test Board";
    private string? _description = "Test description";
    private long _projectId = 1;
    private long _createdBy = 1;
    private bool _isArchived = false;
    private readonly List<Column> _columns = [];

    public BoardBuilder WithId(long id)
    {
        _id = id;
        return this;
    }

    public BoardBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public BoardBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public BoardBuilder WithProjectId(long projectId)
    {
        _projectId = projectId;
        return this;
    }

    public BoardBuilder WithCreatedBy(long userId)
    {
        _createdBy = userId;
        return this;
    }

    public BoardBuilder Archived()
    {
        _isArchived = true;
        return this;
    }

    public BoardBuilder WithColumn(Column column)
    {
        _columns.Add(column);
        return this;
    }

    public Board Build()
    {
        var board = ApplicationTestUtils.CreateInstanceWithoutConstructor<Board>();
        ApplicationTestUtils.InitializeDomainEvents(board);
        ApplicationTestUtils.SetPrivatePropertyValue(board, "Id", _id);
        ApplicationTestUtils.SetPrivatePropertyValue(board, "Name", _name);
        ApplicationTestUtils.SetPrivatePropertyValue(board, "Description", _description);
        ApplicationTestUtils.SetPrivatePropertyValue(board, "ProjectId", _projectId);
        ApplicationTestUtils.SetPrivatePropertyValue(board, "CreatedBy", _createdBy);
        ApplicationTestUtils.SetPrivatePropertyValue(board, "IsArchived", _isArchived);
        ApplicationTestUtils.SetPrivatePropertyValue(board, "_columns", _columns);
        return board;
    }
}
