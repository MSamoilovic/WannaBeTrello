namespace Feezbow.Infrastructure.Options;

public class StorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>Filesystem root under which attachment files are stored. Created on startup if missing.</summary>
    public string LocalPath { get; set; } = "storage";

    /// <summary>Hard upload size cap (megabytes).</summary>
    public int MaxFileSizeMb { get; set; } = 10;

    /// <summary>
    /// Allowed MIME types. Empty list means "allow everything" — useful for dev, not recommended for prod.
    /// </summary>
    public List<string> AllowedContentTypes { get; set; } =
    [
        "application/pdf",
        "image/jpeg",
        "image/png",
        "image/webp",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    ];
}
