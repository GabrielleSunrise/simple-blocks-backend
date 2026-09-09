using Microsoft.EntityFrameworkCore;
using SimpleBlocks.Application.Interfaces.Repositories;
using SimpleBlocks.Domain.Entities;

namespace SimpleBlocks.Infrastructure.Persistence.Repositories;

public class SeedAccountRepository : ISeedAccountRepository
{
    private readonly SimpleBlocksDbContext _db;

    public SeedAccountRepository(SimpleBlocksDbContext db) => _db = db;

    public async Task<SeedAccount?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.SeedAccounts.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<SeedAccount?> GetByPublicKeyAsync(string publicKey, CancellationToken ct = default)
        => await _db.SeedAccounts.FirstOrDefaultAsync(a => a.PublicKey == publicKey, ct);

    public async Task AddAsync(SeedAccount account, CancellationToken ct = default)
    {
        _db.SeedAccounts.Add(account);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(SeedAccount account, CancellationToken ct = default)
    {
        _db.SeedAccounts.Update(account);
        await _db.SaveChangesAsync(ct);
    }
}
