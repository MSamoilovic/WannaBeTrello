using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Application.Features.Labels.GetLabelsByBoard;

public record GetLabelsByBoardQueryResponse(long Id, string Name, string Color)
{
    public static GetLabelsByBoardQueryResponse FromEntity(Label label) =>
        new(label.Id, label.Name, label.Color);
}
