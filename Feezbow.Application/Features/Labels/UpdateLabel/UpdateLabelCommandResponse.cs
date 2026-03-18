namespace WannabeTrello.Application.Features.Labels.UpdateLabel;

public record UpdateLabelCommandResponse(long Id, string Name, string Color, long BoardId);
