using MediatR;

namespace WannabeTrello.Application.Features.Tasks.AddCommentToTask;

public class AddCommentToTaskCommand: IRequest<long>
{
    public long TaskId { get; set; }
    public string Content { get; set; } = null!;
}
