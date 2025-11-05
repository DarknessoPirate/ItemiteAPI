using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class RefreshTokenRepository(ItemiteDbContext context) : IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    // gets all active tokens for user, if deviceId specified it will return only active tokens related to the device
    public async Task<List<RefreshToken>> GetActiveTokensForUserAsync(int userId, string? deviceId = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.IsActive);

        if (!string.IsNullOrEmpty(deviceId))
        {
            query = query.Where(rt => rt.DeviceId == deviceId);
        }

        return await query.ToListAsync(cancellationToken);
    }

    // gets all active tokens for user
    public async Task<List<RefreshToken>> GetAllActiveTokensForUserAsync(int userId,
        CancellationToken cancellationToken = default)
    {
        return await context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    // traverse the chain of tokens and add each to the list, return the list
    public async Task<List<RefreshToken>> GetTokenChainAsync(int tokenId, CancellationToken cancellationToken = default)
    {
        var chain = new List<RefreshToken>();
        var currentToken = await context.RefreshTokens.Include(rt => rt.ReplacedByToken)
            .FirstOrDefaultAsync(rt => rt.Id == tokenId, cancellationToken);

        while (currentToken != null)
        {
            chain.Add(currentToken);
            if (currentToken.ReplacedByTokenId.HasValue)
            {
                currentToken = await context.RefreshTokens.Include(rt => rt.ReplacedByToken).FirstOrDefaultAsync(rt => rt.Id == currentToken.ReplacedByTokenId, cancellationToken);
            }
            else
            {
                currentToken = null;
            }
        }
        
        return chain;
    }

    public async Task CreateAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
       await context.RefreshTokens.AddAsync(token, cancellationToken); 
    }

    public void Update(RefreshToken token, CancellationToken cancellationToken = default)
    {
        context.RefreshTokens.Update(token);
    }
}