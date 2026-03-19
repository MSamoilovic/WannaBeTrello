using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Specifications.ColumnSpecifications;

/// <summary>
/// Specification za dohvatanje svih Columns-a određenog Board-a sa Tasks i Assignee
/// </summary>
public class ColumnsByBoardIdSpecification : BaseSpecification<Column>
{
    public ColumnsByBoardIdSpecification(long boardId) : base(c => c.BoardId == boardId && !c.IsDeleted)
    {
        AddInclude(c => c.Tasks);
        AddInclude("Tasks.Assignee");
        ApplyOrderBy(c => c.Order);
        // No tracking za read-only query
    }
}

