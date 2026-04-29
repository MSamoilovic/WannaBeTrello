namespace Feezbow.Domain.Interfaces.Services;

/// <summary>
/// Storage backend abstraction. Implementations decide how/where the bytes live (local disk, S3, …).
/// Domain code never deals with paths or buckets directly.
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Persists the given content under a backend-generated key inside <paramref name="folder"/>.
    /// Returns the storage key needed to read or delete the file later.
    /// </summary>
    Task<string> SaveAsync(
        string folder,
        string originalFileName,
        Stream content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens a read stream for the given storage key. Caller owns the stream and must dispose it.
    /// </summary>
    Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default);

    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}
