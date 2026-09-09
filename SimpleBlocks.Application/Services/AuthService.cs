using SimpleBlocks.Application.Dtos;
using SimpleBlocks.Application.Interfaces;
using SimpleBlocks.Application.Interfaces.Repositories;
using SimpleBlocks.Domain.Entities;

namespace SimpleBlocks.Application.Services;

public class AuthService : IAuthService
{
    private readonly ISeedAccountRepository _accounts;
    private readonly IChallengeStore _challenges;
    private readonly IWalletCrypto _crypto;
    private readonly ITokenService _tokens;

    private static readonly TimeSpan ChallengeLifetime = TimeSpan.FromMinutes(5);

    public AuthService(
        ISeedAccountRepository accounts,
        IChallengeStore challenges,
        IWalletCrypto crypto,
        ITokenService tokens)
    {
        _accounts = accounts;
        _challenges = challenges;
        _crypto = crypto;
        _tokens = tokens;
    }

    public async Task<AccountInfo> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.PublicKey))
            throw new ArgumentException("PublicKey is required.");

        var key = request.PublicKey.Trim();

        // Registration is idempotent: deriving the same key from the same seed
        // yields the same account, so "logging in with an existing seed"
        // simply returns the existing account.
        var existing = await _accounts.GetByPublicKeyAsync(key, ct);
        if (existing is not null)
            return new AccountInfo(existing.Id, existing.PublicKey, existing.DisplayName, existing.CreatedAtUtc);

        var account = new SeedAccount
        {
            Id = Guid.NewGuid(),
            PublicKey = key,
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? null : request.DisplayName!.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        await _accounts.AddAsync(account, ct);
        return new AccountInfo(account.Id, account.PublicKey, account.DisplayName, account.CreatedAtUtc);
    }

    public async Task<ChallengeResponse> CreateChallengeAsync(Guid accountId, CancellationToken ct = default)
    {
        if (await _accounts.GetByIdAsync(accountId, ct) is null)
            throw new KeyNotFoundException("Account not found.");

        var nonce = await _challenges.CreateAsync(accountId, ChallengeLifetime, ct);
        return new ChallengeResponse(accountId, nonce);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var account = await _accounts.GetByIdAsync(request.AccountId, ct)
            ?? throw new KeyNotFoundException("Account not found.");

        byte[] nonceBytes;
        try
        {
            nonceBytes = Convert.FromBase64String(request.Nonce);
        }
        catch (FormatException)
        {
            throw new ArgumentException("Nonce has an invalid format.");
        }

        // Consume the challenge first (one-time use) to prevent replay attacks.
        var challengeValid = await _challenges.ConsumeAsync(request.AccountId, request.Nonce, ct);
        if (!challengeValid)
            throw new InvalidOperationException("Challenge is invalid or has expired.");

        // Zero-knowledge check: the client proves it holds the private key that
        // matches the registered public key, without revealing any secret.
        if (!_crypto.VerifySignature(account.PublicKey, nonceBytes, request.Signature))
            throw new UnauthorizedAccessException("Signature verification failed.");

        account.LastLoginAtUtc = DateTime.UtcNow;
        await _accounts.UpdateAsync(account, ct);

        var token = _tokens.CreateAccessToken(account);
        var refreshToken = _tokens.CreateRefreshToken(account);
        return new LoginResponse(token, refreshToken, account.Id, account.PublicKey, account.DisplayName);
    }

    public async Task<LoginResponse> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ArgumentException("Refresh token is required.");

        var accountId = _tokens.ValidateRefreshToken(refreshToken)
            ?? throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        var account = await _accounts.GetByIdAsync(accountId, ct)
            ?? throw new UnauthorizedAccessException("Account not found.");

        // Ротация: выдаём новую пару access + refresh.
        var token = _tokens.CreateAccessToken(account);
        var newRefresh = _tokens.CreateRefreshToken(account);
        return new LoginResponse(token, newRefresh, account.Id, account.PublicKey, account.DisplayName);
    }

    public async Task<AccountInfo> GetAccountInfoAsync(Guid accountId, CancellationToken ct = default)
    {
        var account = await _accounts.GetByIdAsync(accountId, ct)
            ?? throw new KeyNotFoundException("Account not found.");

        return new AccountInfo(account.Id, account.PublicKey, account.DisplayName, account.CreatedAtUtc);
    }
}
