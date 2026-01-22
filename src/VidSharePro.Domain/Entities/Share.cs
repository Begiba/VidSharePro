// VidSharePro.Domain/Entities/Share.cs
using VidSharePro.Domain.Common;

public class Share : BaseEntity
{
    public Guid VideoId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime? ExpiryDate { get; private set; }
    public bool IsRevoked { get; private set; }

    public Share(Guid videoId, string token, DateTime? expiryDate)
    {
        if (string.IsNullOrWhiteSpace(token)) throw new ArgumentException("Token is required");
        if (expiryDate.HasValue && expiryDate.Value <= DateTime.UtcNow)
            throw new ArgumentException("Expiration date must be in the future.");

        VideoId = videoId;
        Token = token;
        ExpiryDate = expiryDate;
    }

    /// <summary>
    /// Domain Rule: Validate if the share is still accessible.
    /// </summary>
    public bool IsValid()
    {
        if (IsRevoked) return false;
        if (IsDeleted) return false;
        if (ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow) return false;

        return true;
    }

    public void Revoke() => IsRevoked = true;
}