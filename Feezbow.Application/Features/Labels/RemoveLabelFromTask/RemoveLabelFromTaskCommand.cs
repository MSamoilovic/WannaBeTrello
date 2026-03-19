using MediatR;

namespace Feezbow.Application.Features.Labels.RemoveLabelFromTask;

public record RemoveLabelFromTaskCommand(long TaskId, long LabelId) : IRequest<RemoveLabelFromTaskCommandResponse>;
