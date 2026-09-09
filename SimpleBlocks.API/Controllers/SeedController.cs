using Microsoft.AspNetCore.Mvc;

namespace SimpleBlocks.API.Controllers;

[ApiController]
[Route("api/seed")]
public class SeedController : ControllerBase
{
    private const string ResourceName = "SimpleBlocks.API.Resources.wordlist.txt";

    /// <summary>
    /// Returns the BIP39-style English wordlist used by the client to generate
    /// seed phrases. The generation itself happens entirely on the client — the
    /// server never sees the seed phrase.
    /// </summary>
    [HttpGet("wordlist")]
    public async Task<IActionResult> Wordlist(CancellationToken ct)
    {
        var assembly = typeof(SeedController).Assembly;
        await using var stream = assembly.GetManifestResourceStream(ResourceName);
        if (stream is null)
            return NotFound(new { message = "Wordlist is not available." });

        using var reader = new StreamReader(stream);

        var words = (await reader.ReadToEndAsync(ct))
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(word => word.Trim())
            .Where(word => word.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return Ok(new { wordCount = words.Length, words });
    }
}
