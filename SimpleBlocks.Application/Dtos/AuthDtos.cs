namespace SimpleBlocks.Application.Dtos;

public record RegisterRequest(string PublicKey, string? DisplayName = null);

public record AccountInfo(Guid AccountId, string PublicKey, string? DisplayName, DateTime CreatedAtUtc);

public record ChallengeRequest(Guid AccountId);

public record ChallengeResponse(Guid AccountId, string Nonce);

public record LoginRequest(Guid AccountId, string Nonce, string Signature);

public record LoginResponse(string Token, string RefreshToken, Guid AccountId, string PublicKey, string? DisplayName);

public record RefreshRequest(string RefreshToken);
