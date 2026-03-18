using MediatR;
using WannabeTrello.Domain.Enums;

namespace WannabeTrello.Application.Features.Tasks.UpdateTask;

public class UpdateTaskCommand : IRequest<UpdateTaskCommandResponse>
{
    public long TaskId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime DueDate { get; set; }
}