namespace SimpleBlocks.Domain.Entities;

/// <summary>
/// An anonymous account identified only by its Ed25519 public key
/// (the "address" derived from the user's seed phrase). The seed and the
/// private key are never stored or transmitted to the server.
/// </summary>
public class SeedAccount
{
    public Guid Id { get; set; }

    /// <summary>Base64-encoded 32-byte Ed25519 public key. Unique pseudonymous identifier.</summary>
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>Optional self-chosen pseudonymous display name. No personal data is collected.</summary>
    public string? DisplayName { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAtUtc { get; set; }
}
