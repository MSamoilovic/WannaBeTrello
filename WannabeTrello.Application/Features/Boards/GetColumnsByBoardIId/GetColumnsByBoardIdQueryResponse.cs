namespace WannabeTrello.Application.Features.Boards.GetColumnsByBoardIId;

public class GetColumnsByBoardIdQueryResponse
{
    public long ColumnId { get; set; }
    public string? ColumnName { get; set; }
    public int ColumnOrder { get; set; }
    public int? WipLimit { get; set; }
    public List<TaskResponse>? Tasks { get; set; }
    
    public static GetColumnsByBoardIdQueryResponse FromEntity(Domain.Entities.Column column)
    {
        return new GetColumnsByBoardIdQueryResponse
        {
            ColumnId = column.Id,
            ColumnName = column.Name,
            ColumnOrder = column.Order,
            WipLimit = column.WipLimit,
            Tasks = column.Tasks?.Select(t => new TaskResponse
            {
                TaskId = t.Id,
                Title = t.Title,
                Description = t.Description,
                Priority = t.Priority.ToString(),
                DueDate = t.DueDate,
                
            }).ToList() ?? []
        };
    }
}

public class TaskResponse
{
    public long TaskId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Priority { get; set; }
    public DateTime? DueDate { get; set; }
}