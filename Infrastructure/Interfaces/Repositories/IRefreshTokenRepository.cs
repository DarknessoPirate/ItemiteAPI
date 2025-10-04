using Domain.Entities;

namespace Infrastructure.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    Task<List<RefreshToken>> GetActiveTokensForUserAsync(int userId, string? deviceId = null,
        CancellationToken cancellationToken = default);

    Task<List<RefreshToken>> GetAllActiveTokensForUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<List<RefreshToken>> GetTokenChainAsync(int tokenId, CancellationToken cancellationToken = default);
    Task CreateAsync(RefreshToken token, CancellationToken cancellationToken = default);
    void Update(RefreshToken token, CancellationToken cancellationToken = default);// TODO: change to async if possible
}