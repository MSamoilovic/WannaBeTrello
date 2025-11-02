using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Specifications.ColumnSpecifications;

/// <summary>
/// Specification za dohvatanje detalja Column-a po ID-u sa Tasks i Assignee
/// </summary>
public class ColumnDetailsByIdSpecification : BaseSpecification<Column>
{
    public ColumnDetailsByIdSpecification(long id) : base(c => c.Id == id && !c.IsDeleted)
    {
        AddInclude(c => c.Tasks);
        AddInclude("Tasks.Assignee");
        // No tracking za read-only query
    }
}

