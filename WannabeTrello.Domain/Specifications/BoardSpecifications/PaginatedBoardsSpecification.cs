using WannabeTrello.Domain.Entities;
using WannabeTrello.Domain.Specifications;

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
        
        AddInclude(b => b.BoardMembers);
        
        
        ApplyOrderBy(b => b.Name!);
        
        
        ApplyPaging(skip: (pageNumber - 1) * pageSize, take: pageSize);
    }
}

