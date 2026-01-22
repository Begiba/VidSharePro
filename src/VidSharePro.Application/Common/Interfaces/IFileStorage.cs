// VidSharePro.Application/Common/Interfaces/IFileStorage.cs
namespace VidSharePro.Application.Common.Interfaces;

public interface IFileStorage
{
    /// <summary>
    /// Saves a stream to the local/NAS storage and returns the relative path.
    /// </summary>
    Task<string> SaveFileAsync(Stream fileStream, string fileName, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a file stream for reading (supports range requests).
    /// </summary>
    Task<Stream> GetFileStreamAsync(string path, CancellationToken ct = default);

    Task DeleteFileAsync(string path, CancellationToken ct = default);

    Task<bool> ExistsAsync(string path, CancellationToken ct = default);
}