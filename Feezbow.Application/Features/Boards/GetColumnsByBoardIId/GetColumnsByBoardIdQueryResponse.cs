using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Application.Features.Boards.GetColumnsByBoardIId;

public class GetColumnsByBoardIdQueryResponse
{
    public long ColumnId { get; set; }
    public string? ColumnName { get; set; }
    public int ColumnOrder { get; set; }
    public int? WipLimit { get; set; }
    public List<TaskResponse>? Tasks { get; set; }
    
    public static List<GetColumnsByBoardIdQueryResponse> FromEntity(IReadOnlyList<Column?> column)
    {
        return [.. column.Select(col => new GetColumnsByBoardIdQueryResponse
        {
            ColumnId = col.Id,
            ColumnName = col.Name,
            ColumnOrder = col.Order,
            WipLimit = col.WipLimit,
            Tasks = col.Tasks?.Select(t => new TaskResponse
            {
                TaskId = t.Id,
                Title = t.Title,
                Description = t.Description,
                Priority = t.Priority.ToString(),
                DueDate = t.DueDate,

            }).ToList()
        })];
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