using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Feezbow.Domain.Interfaces.Services;
using Feezbow.Infrastructure.Options;

namespace Feezbow.Infrastructure.Services;

public class LocalFileStorageService(
    IOptions<StorageOptions> options,
    ILogger<LocalFileStorageService> logger) : IStorageService
{
    private readonly StorageOptions _options = options.Value;

    public async Task<string> SaveAsync(
        string folder,
        string originalFileName,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var sanitizedFolder = SanitizeRelativePath(folder);
        var extension = Path.GetExtension(originalFileName);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var storageKey = string.IsNullOrEmpty(sanitizedFolder) ? fileName : $"{sanitizedFolder}/{fileName}";

        var fullPath = ResolveFullPath(storageKey);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var fileStream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(fileStream, cancellationToken);

        logger.LogInformation("Stored attachment {StorageKey} ({Size} bytes)", storageKey, fileStream.Length);
        return storageKey;
    }

    public Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolveFullPath(storageKey);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Attachment not found in storage: {storageKey}");

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolveFullPath(storageKey);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            logger.LogInformation("Deleted attachment {StorageKey}", storageKey);
        }
        return Task.CompletedTask;
    }

    private string ResolveFullPath(string storageKey)
    {
        var sanitizedKey = SanitizeRelativePath(storageKey);
        var fullPath = Path.GetFullPath(Path.Combine(_options.LocalPath, sanitizedKey));
        var rootFullPath = Path.GetFullPath(_options.LocalPath);

        if (!fullPath.StartsWith(rootFullPath, StringComparison.Ordinal))
            throw new InvalidOperationException("Resolved path escapes storage root.");

        return fullPath;
    }

    private static string SanitizeRelativePath(string input)
    {
        return input
            .Replace("..", string.Empty, StringComparison.Ordinal)
            .Replace('\\', '/')
            .TrimStart('/');
    }
}
