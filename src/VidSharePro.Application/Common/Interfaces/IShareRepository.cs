// VidSharePro.Application/Common/Interfaces/IShareRepository.cs
using VidSharePro.Domain.Entities;

namespace VidSharePro.Application.Common.Interfaces;

public interface IShareRepository
{
    Task<Share?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task AddAsync(Share share, CancellationToken ct = default);
    Task DeleteExpiredAsync(CancellationToken ct = default);
}