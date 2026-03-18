using MediatR;

namespace WannabeTrello.Application.Features.Labels.DeleteLabel;

public record DeleteLabelCommand(long LabelId) : IRequest<DeleteLabelCommandResponse>;
