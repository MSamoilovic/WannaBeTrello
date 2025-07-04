using MediatR;
using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Application.Features.Tasks.CreateTask;

public class CreateTaskCommand: IRequest<long>
{
    public long ColumnId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime DueDate { get; set; }
    public int Position { get; set; }
    public long? AssigneeId { get; set; }
}
