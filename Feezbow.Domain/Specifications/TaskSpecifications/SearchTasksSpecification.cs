using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Domain.Specifications.TaskSpecifications;

/// <summary>
/// Specification za OData search tasks-a
/// Omogućava filtriranje i pretragu tasks-a sa uključenim Column, Board, BoardMembers i Comments
/// </summary>
public class SearchTasksSpecification : BaseSpecification<BoardTask>
{
    public SearchTasksSpecification() : base()
    {
        // Include-ujemo potrebne entitete za search i OData filtering
        AddInclude(t => t.Column);
        AddInclude(t => t.Column.Board);
        AddInclude(t => t.Column.Board.BoardMembers);
        AddInclude(t => t.Comments);
        AddInclude(t => t.Assignee);
        
        // No tracking za bolje performanse kod search query-ja
        // AsNoTracking = true je default u BaseSpecification
    }
}

