using System.Text.RegularExpressions;
using WannabeTrello.Domain.Exceptions;

namespace WannabeTrello.Domain.Entities;

public class Label : AuditableEntity
{
    private const int MaxNameLength = 50;
    private static readonly Regex HexColorRegex = new(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", RegexOptions.Compiled);

    public string Name { get; private set; } = string.Empty;
    public string Color { get; private set; } = "#6366f1";
    public long BoardId { get; private set; }
    public Board? Board { get; private set; }

    private Label() { }

    public static Label Create(string name, string color, long boardId, long creatorUserId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleValidationException("Label name cannot be empty.");
        if (name.Length > MaxNameLength)
            throw new BusinessRuleValidationException($"Label name cannot exceed {MaxNameLength} characters.");

        ValidateColor(color);

        return new Label
        {
            Name = name.Trim(),
            Color = color.Trim().ToUpperInvariant(),
            BoardId = boardId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = creatorUserId
        };
    }

    public void Update(string name, string color, long modifierUserId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleValidationException("Label name cannot be empty.");
        if (name.Length > MaxNameLength)
            throw new BusinessRuleValidationException($"Label name cannot exceed {MaxNameLength} characters.");

        ValidateColor(color);

        Name = name.Trim();
        Color = color.Trim().ToUpperInvariant();
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifierUserId;
    }

    private static void ValidateColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            throw new BusinessRuleValidationException("Label color cannot be empty.");
        if (!HexColorRegex.IsMatch(color))
            throw new BusinessRuleValidationException("Label color must be a valid hex color (e.g., #FF5733 or #F53).");
    }
}
