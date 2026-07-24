using GuestService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GuestService.Infrastructure.Persistence;

public class GuestDbContext : DbContext
{
    public GuestDbContext(DbContextOptions<GuestDbContext> options) : base(options)
    {
    }

    public DbSet<Guest> Guests => Set<Guest>();
    public DbSet<CachedParty> CachedParties => Set<CachedParty>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Guest entity
        modelBuilder.Entity<Guest>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Foreign key relationship
            entity.HasOne(e => e.Party)
                .WithMany(p => p.Guests)
                .HasForeignKey(e => e.PartyId)
                .OnDelete(DeleteBehavior.Restrict); // Don't cascade delete
        });

        // Configure CachedParty entity
        modelBuilder.Entity<CachedParty>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.BirthdayChildName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Title)
                .HasMaxLength(100);

            entity.Property(e => e.BirthdayChildPhotoUrl)
                .HasMaxLength(500);

            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("GETUTCDATE()");
        });
    }
}