using SimpleBlocks.Domain.Entities;

namespace SimpleBlocks.Application.Interfaces.Repositories;

public interface ISeedAccountRepository
{
    Task<SeedAccount?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<SeedAccount?> GetByPublicKeyAsync(string publicKey, CancellationToken ct = default);
    Task AddAsync(SeedAccount account, CancellationToken ct = default);
    Task UpdateAsync(SeedAccount account, CancellationToken ct = default);
}
