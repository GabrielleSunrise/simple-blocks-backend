using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimpleBlocks.Application.Interfaces;
using SimpleBlocks.Application.Interfaces.Repositories;
using SimpleBlocks.Infrastructure.Auth;
using SimpleBlocks.Infrastructure.Persistence;
using SimpleBlocks.Infrastructure.Persistence.Repositories;
using SimpleBlocks.Infrastructure.Security;

namespace SimpleBlocks.Infrastructure;

/// <summary>JWT signing/validation options.</summary>
public class JwtOptions
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "SimpleBlocks";
    public string Audience { get; set; } = "SimpleBlocks.Client";
    public int ExpiryMinutes { get; set; } = 60;
    public int RefreshDays { get; set; } = 30;
}

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        JwtOptions jwtOptions)
    {
        services.AddDbContext<SimpleBlocksDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<ISeedAccountRepository, SeedAccountRepository>();
        services.AddScoped<ISavedBlockRepository, SavedBlockRepository>();

        services.AddSingleton<IChallengeStore, InMemoryChallengeStore>();
        services.AddSingleton<IWalletCrypto, Ed25519WalletCrypto>();
        services.AddSingleton<ITokenService>(new JwtTokenService(jwtOptions));

        return services;
    }
}
