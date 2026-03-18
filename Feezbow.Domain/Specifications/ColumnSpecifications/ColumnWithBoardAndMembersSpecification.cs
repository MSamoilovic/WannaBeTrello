using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Specifications.ColumnSpecifications;

/// <summary>
/// Specification za dohvatanje Column-a sa Board i BoardMembers (za autorizaciju)
/// </summary>
public class ColumnWithBoardAndMembersSpecification : BaseSpecification<Column>
{
    public ColumnWithBoardAndMembersSpecification(long id) : base(c => c.Id == id && !c.IsDeleted)
    {
        AddInclude(c => c.Board);
        AddInclude(c => c.Board.BoardMembers);
        // Tracking potreban za update/delete operacije
        ApplyTracking();
    }
}

