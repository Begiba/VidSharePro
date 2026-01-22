// VidSharePro.Application/Services/UserService.cs
using VidSharePro.Application.Common.Interfaces;
using VidSharePro.Domain.Entities;
using VidSharePro.Application.DTOs;

namespace VidSharePro.Application.Services;

public class UserService(IUserRepository userRepository, IAuthService cryptoProvider)
{
    public async Task RegisterAsync(RegisterRequestDto request, CancellationToken ct)
    {
        var existing = await userRepository.GetByEmailAsync(request.Email, ct);
        if (existing != null) throw new InvalidOperationException("Email already registered.");

        var hashedPassword = cryptoProvider.HashPassword(request.Password);
        var user = new User(request.Username, request.Email, hashedPassword);

        await userRepository.AddAsync(user, ct);
    }
}