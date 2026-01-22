// VidSharePro.Application/Services/AuthService.cs
using VidSharePro.Application.Common.Interfaces;
using VidSharePro.Application.DTOs;
using VidSharePro.Domain.Entities;

namespace VidSharePro.Application.Services;

public class AuthService(IUserRepository userRepository, IAuthService cryptoProvider)
{
    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, ct);

        if (user == null || !cryptoProvider.VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is deactivated.");

        var token = cryptoProvider.GenerateJwtToken(user);

        return new AuthResponseDto(user.Username, user.Email, user.Role.ToString(), token);
    }
}