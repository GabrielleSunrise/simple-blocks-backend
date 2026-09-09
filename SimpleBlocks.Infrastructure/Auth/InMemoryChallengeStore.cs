using System.Collections.Concurrent;
using System.Security.Cryptography;
using SimpleBlocks.Application.Interfaces;

namespace SimpleBlocks.Infrastructure.Auth;

/// <summary>
/// In-memory, single-use, expiring challenge store. Sufficient for a single
/// API instance; for horizontal scaling on a VPS it can be swapped for a
/// shared store (Redis / DB) behind the same IChallengeStore interface.
/// </summary>
public class InMemoryChallengeStore : IChallengeStore
{
    private sealed record Entry(string Key, DateTime ExpiresUtc);

    private readonly ConcurrentDictionary<Guid, Entry> _challenges = new();

    public Task<string> CreateAsync(Guid accountId, TimeSpan lifetime, CancellationToken ct = default)
    {
        var nonce = new byte[32];
        RandomNumberGenerator.Fill(nonce);

        var key = Convert.ToBase64String(nonce);
        _challenges[accountId] = new Entry(key, DateTime.UtcNow.Add(lifetime));

        return Task.FromResult(key);
    }

    public Task<bool> ConsumeAsync(Guid accountId, string nonce, CancellationToken ct = default)
    {
        if (!_challenges.TryRemove(accountId, out var entry))
            return Task.FromResult(false);

        if (entry.ExpiresUtc < DateTime.UtcNow)
            return Task.FromResult(false);

        return Task.FromResult(string.Equals(entry.Key, nonce, StringComparison.Ordinal));
    }
}
