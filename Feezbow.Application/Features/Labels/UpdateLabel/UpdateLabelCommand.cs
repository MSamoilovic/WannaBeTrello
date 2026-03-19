using MediatR;

namespace Feezbow.Application.Features.Labels.UpdateLabel;

public record UpdateLabelCommand(long LabelId, string Name, string Color) : IRequest<UpdateLabelCommandResponse>;
