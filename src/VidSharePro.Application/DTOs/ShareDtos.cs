// VidSharePro.Application/DTOs/ShareDtos.cs
namespace VidSharePro.Application.DTOs;

public record CreateShareRequestDto(Guid VideoId, DateTime? ExpiryDate);