using MediatR;

namespace WannabeTrello.Application.Features.Labels.AddLabelToTask;

public record AddLabelToTaskCommand(long TaskId, long LabelId) : IRequest<AddLabelToTaskCommandResponse>;
