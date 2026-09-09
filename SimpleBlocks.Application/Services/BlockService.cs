using System.Text.Json;
using SimpleBlocks.Application.Common;
using SimpleBlocks.Application.Dtos;
using SimpleBlocks.Application.Interfaces;
using SimpleBlocks.Application.Interfaces.Repositories;
using SimpleBlocks.Domain.Entities;

namespace SimpleBlocks.Application.Services;

public class BlockService : IBlockService
{
    private readonly ISavedBlockRepository _blocks;

    public BlockService(ISavedBlockRepository blocks) => _blocks = blocks;

    public async Task<List<BlockDto>> ListAsync(Guid ownerId, CancellationToken ct = default)
    {
        var blocks = await _blocks.ListByOwnerAsync(ownerId, ct);
        return blocks.Select(ToDto).ToList();
    }

    public async Task<BlockDto> SaveAsync(Guid ownerId, SaveBlockRequest request, CancellationToken ct = default)
    {
        Validate(request);

        var existing = await _blocks.ListByOwnerAsync(ownerId, ct);
        var nextOrder = existing.Count == 0 ? 0 : existing.Max(b => b.SortOrder) + 1;

        var block = new SavedBlock
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            BlockType = NormalizeType(request.BlockType),
            Name = (request.Name ?? string.Empty).Trim(),
            SettingsJson = SerializeSettings(request.Settings),
            Html = request.Html ?? string.Empty,
            Css = request.Css ?? string.Empty,
            Js = request.Js ?? string.Empty,
            SortOrder = nextOrder,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await _blocks.AddAsync(block, ct);
        return ToDto(block);
    }

    public async Task<BlockDto> UpdateAsync(Guid ownerId, Guid blockId, SaveBlockRequest request, CancellationToken ct = default)
    {
        Validate(request);

        var block = await _blocks.GetByIdAsync(blockId, ct)
            ?? throw new KeyNotFoundException("Block not found.");

        if (block.OwnerId != ownerId)
            throw new KeyNotFoundException("Block not found.");

        block.BlockType = NormalizeType(request.BlockType);
        block.Name = (request.Name ?? string.Empty).Trim();
        block.SettingsJson = SerializeSettings(request.Settings);
        block.Html = request.Html ?? string.Empty;
        block.Css = request.Css ?? string.Empty;
        block.Js = request.Js ?? string.Empty;
        block.UpdatedAtUtc = DateTime.UtcNow;

        await _blocks.UpdateAsync(block, ct);
        return ToDto(block);
    }

    public async Task DeleteAsync(Guid ownerId, Guid blockId, CancellationToken ct = default)
    {
        var block = await _blocks.GetByIdAsync(blockId, ct)
            ?? throw new KeyNotFoundException("Block not found.");

        if (block.OwnerId != ownerId)
            throw new KeyNotFoundException("Block not found.");

        await _blocks.DeleteAsync(block, ct);
    }

    public async Task ReorderAsync(Guid ownerId, IReadOnlyList<Guid> orderedIds, CancellationToken ct = default)
    {
        if (orderedIds is null || orderedIds.Count == 0)
            return;

        var blocks = await _blocks.ListByOwnerAsync(ownerId, ct);
        var byId = blocks.ToDictionary(b => b.Id);

        for (var i = 0; i < orderedIds.Count; i++)
        {
            if (byId.TryGetValue(orderedIds[i], out var block))
                block.SortOrder = i;
        }

        await _blocks.UpdateRangeAsync(byId.Values.Where(b => b.SortOrder >= 0 && b.SortOrder < orderedIds.Count), ct);
    }

    private static void Validate(SaveBlockRequest request)
    {
        if (!BlockTypes.IsValid(request.BlockType))
            throw new ArgumentException($"Unknown block type '{request.BlockType}'.");
    }

    private static string NormalizeType(string type)
        => BlockTypes.All.First(t => string.Equals(t, type, StringComparison.OrdinalIgnoreCase));

    private static string SerializeSettings(JsonElement settings)
        => settings.ValueKind == JsonValueKind.Undefined ? "{}" : JsonSerializer.Serialize(settings);

    private static BlockDto ToDto(SavedBlock block)
    {
        var settings = string.IsNullOrWhiteSpace(block.SettingsJson)
            ? default
            : JsonSerializer.Deserialize<JsonElement>(block.SettingsJson);

        return new BlockDto(
            block.Id,
            block.BlockType,
            block.Name,
            settings,
            block.Html ?? string.Empty,
            block.Css ?? string.Empty,
            block.Js ?? string.Empty,
            block.SortOrder,
            block.CreatedAtUtc,
            block.UpdatedAtUtc);
    }
}
