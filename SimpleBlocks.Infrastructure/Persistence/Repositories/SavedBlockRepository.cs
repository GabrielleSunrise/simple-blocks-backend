using Microsoft.EntityFrameworkCore;
using SimpleBlocks.Application.Interfaces.Repositories;
using SimpleBlocks.Domain.Entities;

namespace SimpleBlocks.Infrastructure.Persistence.Repositories;

public class SavedBlockRepository : ISavedBlockRepository
{
    private readonly SimpleBlocksDbContext _db;

    public SavedBlockRepository(SimpleBlocksDbContext db) => _db = db;

    public async Task<List<SavedBlock>> ListByOwnerAsync(Guid ownerId, CancellationToken ct = default)
        => await _db.SavedBlocks
            .Where(b => b.OwnerId == ownerId)
            .OrderBy(b => b.SortOrder)
            .ThenByDescending(b => b.UpdatedAtUtc)
            .ToListAsync(ct);

    public async Task<SavedBlock?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.SavedBlocks.FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task AddAsync(SavedBlock block, CancellationToken ct = default)
    {
        _db.SavedBlocks.Add(block);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(SavedBlock block, CancellationToken ct = default)
    {
        _db.SavedBlocks.Update(block);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateRangeAsync(IEnumerable<SavedBlock> blocks, CancellationToken ct = default)
    {
        _db.SavedBlocks.UpdateRange(blocks);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(SavedBlock block, CancellationToken ct = default)
    {
        _db.SavedBlocks.Remove(block);
        await _db.SaveChangesAsync(ct);
    }
}
