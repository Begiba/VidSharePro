// VidSharePro.Application/DTOs/AuthDtos.cs
namespace VidSharePro.Application.DTOs;

public record LoginRequestDto(string Email, string Password);

public record RegisterRequestDto(string Username, string Email, string Password);

public record AuthResponseDto(string Username, string Email, string Role, string Token);



