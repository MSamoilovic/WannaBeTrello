using MediatR;

namespace Feezbow.Application.Features.Labels.DeleteLabel;

public record DeleteLabelCommand(long LabelId) : IRequest<DeleteLabelCommandResponse>;
