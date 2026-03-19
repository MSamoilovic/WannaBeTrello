namespace Feezbow.Application.Features.Labels.CreateLabel;

public record CreateLabelCommandResponse(long Id, string Name, string Color, long BoardId);
