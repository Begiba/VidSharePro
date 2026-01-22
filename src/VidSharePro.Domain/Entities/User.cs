// VidSharePro.Domain/Entities/User.cs
using VidSharePro.Domain.Common;
using VidSharePro.Domain.Enums;

namespace VidSharePro.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; } = true;
    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
    private User() { } // EF Core requirement

    public User(string username, string email, string passwordHash, UserRole role = UserRole.User)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username is required");
        if (!email.Contains('@')) throw new ArgumentException("Invalid email format");

        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
    }

    /// <summary>
    /// Domain Rule: User must be active to perform uploads.
    /// </summary>
    public void EnsureCanUpload()
    {
        if (!IsActive)
            throw new InvalidOperationException("Inactive users cannot upload videos.");

        if (IsDeleted)
            throw new InvalidOperationException("Deleted user accounts cannot perform this action.");
    }
}