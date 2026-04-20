using AbsencePlanner.Core.Interfaces;
using AbsencePlanner.Core.Models;

namespace AbsencePlanner.Infrastructure.Services;

public class TokenRevocationService : ITokenRevocationService
{
    private readonly IFirestoreRepository _repo;
    private const string Col = "revoked_tokens";

    public TokenRevocationService(IFirestoreRepository repo) => _repo = repo;

    public async Task RevokeAsync(string userId)
 {
await _repo.SetAsync(Col, userId, new RevokedToken
        {
 UserId = userId,
  RevokedAt = DateTime.UtcNow
 });
    }

    public async Task<bool> IsRevokedAsync(string userId)
    {
        var record = await _repo.GetAsync<RevokedToken>(Col, userId);
        return record is not null;
    }
}
