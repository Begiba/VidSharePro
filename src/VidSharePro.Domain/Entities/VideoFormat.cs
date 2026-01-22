// VidSharePro.Domain/Entities/VideoFormat.cs
using VidSharePro.Domain.Common;

namespace VidSharePro.Domain.Entities;
public class VideoFormat : BaseEntity
{
    public Guid VideoId { get; private set; }
    public string ResolutionLabel { get; private set; } = null!; // e.g. "Original", "1080p"
    public string StoragePath { get; private set; } = null!;
    public string MimeType { get; private set; } = null!;
    public long SizeBytes { get; private set; }

    // 1. MUST have a parameterless constructor (can be private)
    private VideoFormat() { }

    internal VideoFormat(Guid videoId, string label, string path, string mime, long size)
    {
        VideoId = videoId;
        ResolutionLabel = label;
        StoragePath = path;
        MimeType = mime;
        SizeBytes = size;
    }
}