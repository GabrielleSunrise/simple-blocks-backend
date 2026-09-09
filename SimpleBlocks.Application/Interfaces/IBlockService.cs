using SimpleBlocks.Application.Dtos;

namespace SimpleBlocks.Application.Interfaces;

public interface IBlockService
{
    Task<List<BlockDto>> ListAsync(Guid ownerId, CancellationToken ct = default);
    Task<BlockDto> SaveAsync(Guid ownerId, SaveBlockRequest request, CancellationToken ct = default);
    Task<BlockDto> UpdateAsync(Guid ownerId, Guid blockId, SaveBlockRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid ownerId, Guid blockId, CancellationToken ct = default);

    /// <summary>Persists the user-defined order of the account's blocks.</summary>
    Task ReorderAsync(Guid ownerId, IReadOnlyList<Guid> orderedIds, CancellationToken ct = default);
}
