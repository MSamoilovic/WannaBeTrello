using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Specification;

namespace WannabeTrello.Domain.Specifications.BoardSpecifications;

/// <summary>
/// Specification za paginaciju Board-ova projekta
/// </summary>
public class PaginatedBoardsSpecification : BaseSpecification<Board>
{
    public PaginatedBoardsSpecification(
        long projectId, 
        int pageNumber, 
        int pageSize,
        string? searchTerm = null) 
        : base(b => b.ProjectId == projectId && 
                    !b.IsArchived &&
                    (string.IsNullOrEmpty(searchTerm) || 
                     b.Name!.Contains(searchTerm) || 
                     (b.Description != null && b.Description.Contains(searchTerm))))
    {
        // Include members za prikaz
        AddInclude(b => b.BoardMembers);
        
        // Sortiranje po imenu
        ApplyOrderBy(b => b.Name!);
        
        // Paginacija
        ApplyPaging(skip: (pageNumber - 1) * pageSize, take: pageSize);
    }
}

