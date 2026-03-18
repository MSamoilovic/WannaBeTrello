using MediatR;

namespace WannabeTrello.Application.Features.Labels.UpdateLabel;

public record UpdateLabelCommand(long LabelId, string Name, string Color) : IRequest<UpdateLabelCommandResponse>;
