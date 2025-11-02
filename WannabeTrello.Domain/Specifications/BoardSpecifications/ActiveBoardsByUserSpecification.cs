using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Specifications;

namespace WannabeTrello.Domain.Specifications.BoardSpecifications;

/// <summary>
/// Specification za dohvatanje aktivnih Board-ova na kojima je korisnik član
/// </summary>
public class ActiveBoardsByUserSpecification : BaseSpecification<Board>
{
    public ActiveBoardsByUserSpecification(long userId) 
        : base(b => !b.IsArchived && b.BoardMembers.Any(bm => bm.UserId == userId))
    {
        // Include project i board members
        AddInclude(b => b.Project);
        AddInclude(b => b.BoardMembers);
        
        // Sortiranje po poslednjoj modifikaciji (najaktivniji na vrhu)
        ApplyOrderByDescending(b => b.LastModifiedAt ?? b.CreatedAt);
    }
}

