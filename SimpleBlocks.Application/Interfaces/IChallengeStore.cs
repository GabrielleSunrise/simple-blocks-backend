namespace SimpleBlocks.Application.Interfaces;

/// <summary>
/// Issues one-time, expiring cryptographic challenges used to prove that a
/// client holds the private key matching a registered public key — without
/// ever sending private material over the wire.
/// </summary>
public interface IChallengeStore
{
    /// <summary>Creates a random nonce for the given account and returns it as a Base64 string.</summary>
    Task<string> CreateAsync(Guid accountId, TimeSpan lifetime, CancellationToken ct = default);

    /// <summary>
    /// Atomically consumes (invalidates) a nonce. Returns true only if the nonce
    /// exists, has not expired and matches the account. Used to prevent replays.
    /// </summary>
    Task<bool> ConsumeAsync(Guid accountId, string nonce, CancellationToken ct = default);
}
