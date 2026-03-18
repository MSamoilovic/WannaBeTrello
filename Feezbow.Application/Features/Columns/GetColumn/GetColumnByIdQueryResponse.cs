using MediatR;
using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Application.Features.Columns.GetColumn;

public class GetColumnByIdQueryResponse
{
    public long ColumnId { get; set; }
    public string? ColumnName { get; set; }
    public int ColumnOrder { get; set; }
    public int? WipLimit { get; set; }
    public List<TaskResponse>? Tasks { get; set; }
    
    public static GetColumnByIdQueryResponse FromEntity(Column colum)
    {
        return new GetColumnByIdQueryResponse
        {
            ColumnId = colum.Id,
            ColumnName = colum.Name,
            ColumnOrder = colum.Order,
            WipLimit = colum.WipLimit,
            Tasks = colum.Tasks?.Select(t => new TaskResponse
            {
                TaskId = t.Id,
                Title = t.Title,
                Description = t.Description,
                Priority = t.Priority.ToString(),
                DueDate = t.DueDate,
                
            }).ToList() ?? []
        };
    }
    
    public class TaskResponse
    {
        public long TaskId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Priority { get; set; }
        public DateTime? DueDate { get; set; }
    }
}