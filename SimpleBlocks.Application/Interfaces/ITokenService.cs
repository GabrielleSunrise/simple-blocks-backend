using SimpleBlocks.Domain.Entities;

namespace SimpleBlocks.Application.Interfaces;

/// <summary>Creates signed tokens and validates refresh tokens for an account.</summary>
public interface ITokenService
{
    string CreateAccessToken(SeedAccount account);

    /// <summary>Long-lived token used to obtain new access tokens without re-login.</summary>
    string CreateRefreshToken(SeedAccount account);

    /// <summary>Returns the account id encoded in a valid refresh token, or null if invalid/expired.</summary>
    Guid? ValidateRefreshToken(string token);
}
