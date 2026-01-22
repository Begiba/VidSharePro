// VidSharePro.Application/Common/Interfaces/IAuthService.cs
using VidSharePro.Domain.Entities;

namespace VidSharePro.Application.Common.Interfaces;

public interface IAuthService
{
    string GenerateJwtToken(User user);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}