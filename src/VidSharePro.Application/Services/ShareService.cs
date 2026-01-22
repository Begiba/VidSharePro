// VidSharePro.Application/Services/ShareService.cs
using VidSharePro.Application.Common.Interfaces;

namespace VidSharePro.Application.Services;

public class ShareService(IShareRepository shareRepository, IVideoRepository videoRepository)
{
    public async Task<string> CreateShareLinkAsync(Guid videoId, DateTime? expiry, CancellationToken ct)
    {
        var video = await videoRepository.GetByIdAsync(videoId, false, ct)
                    ?? throw new KeyNotFoundException("Video not found");

        // Generate a cryptographically secure token
        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("/", "_").Replace("+", "-").TrimEnd('=');

        var share = new Share(videoId, token, expiry);
        await shareRepository.AddAsync(share, ct);

        return token;
    }

    public async Task<Guid> ValidateShareTokenAsync(string token, CancellationToken ct)
    {
        var share = await shareRepository.GetByTokenAsync(token, ct)
                    ?? throw new UnauthorizedAccessException("Invalid or expired link.");

        if (!share.IsValid())
            throw new UnauthorizedAccessException("This share link has expired.");

        return share.VideoId;
    }
}