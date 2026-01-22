// VidSharePro.Infrastructure/Storage/LocalFileStorage.cs
using Microsoft.Extensions.Configuration;
using VidSharePro.Application.Common.Interfaces;

namespace VidSharePro.Infrastructure.Storage;

public class LocalFileStorage : IFileStorage
{
    private readonly string _basePath;

    public LocalFileStorage(IConfiguration configuration)
    {
        // Path configured in appsettings.json, e.g., "Storage:LocalPath": "C:\\VidShareData"
        _basePath = configuration["Storage:Path"]
            ?? throw new ArgumentNullException("Storage path not configured.");

        if (!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        // Domain Rule: Generate a unique, safe filename to prevent overwrites or path injection
        var trustedFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var relativePath = Path.Combine(DateTime.UtcNow.ToString("yyyy/MM/dd"), trustedFileName);
        var fullPath = Path.Combine(_basePath, relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        using var targetStream = File.Create(fullPath);
        await fileStream.CopyToAsync(targetStream, ct);

        return relativePath; // Store relative path in DB to keep storage portable
    }

    public Task<Stream> GetFileStreamAsync(string path, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_basePath, path);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Video file not found on storage.");

        // We return a FileStream which the API layer will use for Range Requests
        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);

        return Task.FromResult<Stream>(stream);
    }

    public Task DeleteFileAsync(string path, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_basePath, path);
        if (File.Exists(fullPath)) File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string path, CancellationToken ct = default)
        => Task.FromResult(File.Exists(Path.Combine(_basePath, path)));
}