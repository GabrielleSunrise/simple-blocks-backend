namespace SimpleBlocks.Application.Interfaces;

/// <summary>
/// Verifies Ed25519 signatures over the given data using a registered public key.
/// The server only ever holds public keys, so it can verify — but never impersonate —
/// the account owning the seed phrase.
/// </summary>
public interface IWalletCrypto
{
    bool VerifySignature(string publicKeyBase64, byte[] data, string signatureBase64);
}
