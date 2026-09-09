using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleBlocks.Application.Dtos;
using SimpleBlocks.Application.Interfaces;

namespace SimpleBlocks.API.Controllers;

[Authorize]
[ApiController]
[Route("api/blocks")]
public class BlocksController : ControllerBase
{
    private readonly IBlockService _blocks;

    public BlocksController(IBlockService blocks) => _blocks = blocks;

    [HttpPost("reorder")]
    public async Task<IActionResult> Reorder([FromBody] ReorderRequest request, CancellationToken ct)
    {
        var ownerId = GetCurrentAccountId();
        if (ownerId is null) return Unauthorized();

        await _blocks.ReorderAsync(ownerId.Value, request?.Ids ?? Array.Empty<Guid>(), ct);
        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var ownerId = GetCurrentAccountId();
        if (ownerId is null) return Unauthorized();

        return Ok(await _blocks.ListAsync(ownerId.Value, ct));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveBlockRequest request, CancellationToken ct)
    {
        var ownerId = GetCurrentAccountId();
        if (ownerId is null) return Unauthorized();

        try
        {
            var block = await _blocks.SaveAsync(ownerId.Value, request, ct);
            return CreatedAtAction(nameof(List), new { id = block.Id }, block);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SaveBlockRequest request, CancellationToken ct)
    {
        var ownerId = GetCurrentAccountId();
        if (ownerId is null) return Unauthorized();

        try
        {
            var block = await _blocks.UpdateAsync(ownerId.Value, id, request, ct);
            return Ok(block);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Block not found." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var ownerId = GetCurrentAccountId();
        if (ownerId is null) return Unauthorized();

        try
        {
            await _blocks.DeleteAsync(ownerId.Value, id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Block not found." });
        }
    }

    private Guid? GetCurrentAccountId()
    {
        var raw = User.FindFirst("accountId")?.Value;
        return Guid.TryParse(raw, out var id) ? id : null;
    }
}
