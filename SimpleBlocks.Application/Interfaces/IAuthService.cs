using SimpleBlocks.Application.Dtos;

namespace SimpleBlocks.Application.Interfaces;

public interface IAuthService
{
    /// <summary>Registers a new anonymous account from its public key (or returns the existing one).</summary>
    Task<AccountInfo> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

    Task<ChallengeResponse> CreateChallengeAsync(Guid accountId, CancellationToken ct = default);

    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);

    /// <summary>Exchanges a valid refresh token for a new access/refresh pair.</summary>
    Task<LoginResponse> RefreshAsync(string refreshToken, CancellationToken ct = default);

    Task<AccountInfo> GetAccountInfoAsync(Guid accountId, CancellationToken ct = default);
}
