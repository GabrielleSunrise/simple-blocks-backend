using NSec.Cryptography;
using SimpleBlocks.Application.Interfaces;

namespace SimpleBlocks.Infrastructure.Security;

/// <summary>
/// Verifies Ed25519 signatures using the registered public key. The server
/// never holds the private key or the seed phrase, so it can authenticate
/// a client while remaining unable to impersonate it — even with full
/// access to the database.
/// </summary>
public class Ed25519WalletCrypto : IWalletCrypto
{
    private static readonly SignatureAlgorithm Algorithm = SignatureAlgorithm.Ed25519;

    public bool VerifySignature(string publicKeyBase64, byte[] data, string signatureBase64)
    {
        try
        {
            var publicKeyBytes = Convert.FromBase64String(publicKeyBase64);
            var signatureBytes = Convert.FromBase64String(signatureBase64);

            if (signatureBytes.Length != Algorithm.SignatureSize)
                return false;

            var publicKey = PublicKey.Import(Algorithm, publicKeyBytes, KeyBlobFormat.RawPublicKey);
            return Algorithm.Verify(publicKey, data, signatureBytes);
        }
        catch
        {
            return false;
        }
    }
}
