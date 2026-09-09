using Microsoft.EntityFrameworkCore;
using SimpleBlocks.Domain.Entities;

namespace SimpleBlocks.Infrastructure.Persistence;

public class SimpleBlocksDbContext : DbContext
{
    public SimpleBlocksDbContext(DbContextOptions<SimpleBlocksDbContext> options) : base(options)
    {
    }

    public DbSet<SeedAccount> SeedAccounts => Set<SeedAccount>();
    public DbSet<SavedBlock> SavedBlocks => Set<SavedBlock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SeedAccount>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.PublicKey)
                .IsRequired()
                .HasMaxLength(128);

            entity.HasIndex(e => e.PublicKey).IsUnique();
        });

        modelBuilder.Entity<SavedBlock>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.BlockType).IsRequired().HasMaxLength(32);
            entity.Property(e => e.Name).HasMaxLength(200);

            entity.Property(e => e.SettingsJson).HasColumnType("text");
            entity.Property(e => e.Html).HasColumnType("text");
            entity.Property(e => e.Css).HasColumnType("text");
            entity.Property(e => e.Js).HasColumnType("text");

            entity.HasOne(e => e.Owner)
                .WithMany()
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.OwnerId);
        });
    }
}
