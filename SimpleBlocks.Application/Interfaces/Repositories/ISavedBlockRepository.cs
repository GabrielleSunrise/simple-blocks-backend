using SimpleBlocks.Domain.Entities;

namespace SimpleBlocks.Application.Interfaces.Repositories;

public interface ISavedBlockRepository
{
    Task<List<SavedBlock>> ListByOwnerAsync(Guid ownerId, CancellationToken ct = default);
    Task<SavedBlock?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(SavedBlock block, CancellationToken ct = default);
    Task UpdateAsync(SavedBlock block, CancellationToken ct = default);
    Task UpdateRangeAsync(IEnumerable<SavedBlock> blocks, CancellationToken ct = default);
    Task DeleteAsync(SavedBlock block, CancellationToken ct = default);
}
