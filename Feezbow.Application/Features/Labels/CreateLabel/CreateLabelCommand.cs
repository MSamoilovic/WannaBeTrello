using MediatR;

namespace Feezbow.Application.Features.Labels.CreateLabel;

public record CreateLabelCommand(long BoardId, string Name, string Color) : IRequest<CreateLabelCommandResponse>;
