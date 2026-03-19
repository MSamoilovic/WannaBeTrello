using MediatR;

namespace Feezbow.Application.Features.Labels.AddLabelToTask;

public record AddLabelToTaskCommand(long TaskId, long LabelId) : IRequest<AddLabelToTaskCommandResponse>;
