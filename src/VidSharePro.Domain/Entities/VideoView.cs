// VidSharePro.Domain/Entities/VideoView.cs
using VidSharePro.Domain.Common;
using VidSharePro.Domain.Enums;

namespace VidSharePro.Domain.Entities;


public class VideoView : BaseEntity
{
    public Guid VideoId { get; private set; }
    public string? IpAddress { get; private set; }

    // 1. MUST have a parameterless constructor (can be private)
    private VideoView() { }

    public VideoView(Video video, string? ip)
    {
        if (video.Status != VideoStatus.Ready)
            throw new InvalidOperationException("Cannot record views for videos that are not in Ready state.");

        VideoId = video.Id;
        IpAddress = ip;
    }
}
