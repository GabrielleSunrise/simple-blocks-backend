using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleBlocks.Application.Dtos;
using SimpleBlocks.Application.Interfaces;

namespace SimpleBlocks.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    /// <summary>
    /// Registers a new anonymous account from its Ed25519 public key.
    /// Safe to call repeatedly with the same key (idempotent).
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        try
        {
            var account = await _auth.RegisterAsync(request, ct);
            return Ok(account);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Requests a one-time challenge (nonce) for a login attempt.</summary>
    [HttpPost("challenge")]
    public async Task<IActionResult> Challenge([FromBody] ChallengeRequest request, CancellationToken ct)
    {
        try
        {
            var challenge = await _auth.CreateChallengeAsync(request.AccountId, ct);
            return Ok(challenge);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Account not found." });
        }
    }

    /// <summary>Completes login by presenting a signature over the challenge nonce.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _auth.LoginAsync(request, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Account not found." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Signature verification failed." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Exchanges a valid refresh token for a new access/refresh pair.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _auth.RefreshAsync(request.RefreshToken, ct);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Invalid or expired refresh token." });
        }
    }

    /// <summary>Returns the currently authenticated (anonymous) account.</summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var accountId = GetCurrentAccountId();
        if (accountId is null)
            return Unauthorized();

        var account = await _auth.GetAccountInfoAsync(accountId.Value, ct);
        return Ok(account);
    }

    private Guid? GetCurrentAccountId()
    {
        var raw = User.FindFirst("accountId")?.Value;
        return Guid.TryParse(raw, out var id) ? id : null;
    }
}
