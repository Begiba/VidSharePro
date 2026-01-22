// VidSharePro.Application/DTOs/VideoDtos.cs
namespace VidSharePro.Application.DTOs;

public class VideoDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }

    // We do NOT expose the full physical path (StoragePath) to the UI 
    // for security. We only use it internally in the Service.
}

public record UploadVideoRequestDto(
    string Title,
    string FileName,
    long FileSize,
    string ContentType,
    Stream FileStream);

public record VideoResponseDto(
    Guid Id,
    string Title,
    long SizeBytes,
    string Status);