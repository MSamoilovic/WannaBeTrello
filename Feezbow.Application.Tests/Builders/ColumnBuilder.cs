using Feezbow.Application.Tests.Utils;
using Feezbow.Domain.Entities;

namespace Feezbow.Application.Tests.Builders;

public class ColumnBuilder
{
    private long _id = 1;
    private string _name = "Test Column";
    private int _order = 1;
    private long _boardId = 1;
    private int? _wipLimit = null;
    private bool _isDeleted = false;
    private readonly List<BoardTask> _tasks = [];

    public ColumnBuilder WithId(long id)
    {
        _id = id;
        return this;
    }

    public ColumnBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ColumnBuilder WithOrder(int order)
    {
        _order = order;
        return this;
    }

    public ColumnBuilder WithBoardId(long boardId)
    {
        _boardId = boardId;
        return this;
    }

    public ColumnBuilder WithWipLimit(int wipLimit)
    {
        _wipLimit = wipLimit;
        return this;
    }

    public ColumnBuilder Deleted()
    {
        _isDeleted = true;
        return this;
    }

    public ColumnBuilder WithTask(BoardTask task)
    {
        _tasks.Add(task);
        return this;
    }

    public Column Build()
    {
        var column = ApplicationTestUtils.CreateInstanceWithoutConstructor<Column>();
        ApplicationTestUtils.InitializeDomainEvents(column);
        ApplicationTestUtils.SetPrivatePropertyValue(column, "Id", _id);
        ApplicationTestUtils.SetPrivatePropertyValue(column, "Name", _name);
        ApplicationTestUtils.SetPrivatePropertyValue(column, "Order", _order);
        ApplicationTestUtils.SetPrivatePropertyValue(column, "BoardId", _boardId);
        ApplicationTestUtils.SetPrivatePropertyValue(column, "WipLimit", _wipLimit);
        ApplicationTestUtils.SetPrivatePropertyValue(column, "IsDeleted", _isDeleted);
        ApplicationTestUtils.SetPrivatePropertyValue(column, "_tasks", _tasks);
        return column;
    }
}
