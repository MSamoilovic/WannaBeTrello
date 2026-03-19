namespace Feezbow.Domain.Entities;

public class BoardTaskLabel
{
    public long TaskId { get; set; }
    public BoardTask? Task { get; set; }

    public long LabelId { get; set; }
    public Label? Label { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}
