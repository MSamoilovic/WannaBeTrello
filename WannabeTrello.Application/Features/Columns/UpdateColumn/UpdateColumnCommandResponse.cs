using WannabeTrello.Domain.Entities;

namespace WannabeTrello.Application.Features.Columns.UpdateColumn;

public class UpdateColumnCommandResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? WipLimit { get; set; }
    public int Order { get; set; }

    public static UpdateColumnCommandResponse FromEntity(Column column)
    {
        return new UpdateColumnCommandResponse
        {
            Id = column.Id,
            Name = column.Name?? "",
            WipLimit = column.WipLimit,
            Order = column.Order
        };
    }
}