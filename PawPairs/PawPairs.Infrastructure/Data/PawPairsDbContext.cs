using Microsoft.EntityFrameworkCore;
using PawPairs.Domain.Entities;

namespace PawPairs.Infrastructure.Data;

public class PawPairsDbContext : DbContext
{
    public PawPairsDbContext(DbContextOptions<PawPairsDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<MatchRequest> MatchRequests => Set<MatchRequest>();
    public DbSet<Playdate> Playdates => Set<Playdate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Pets)
            .WithOne(p => p.Owner)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MatchRequest>(e =>
        {
            e.HasKey(x => x.Id);

            e.HasOne(x => x.FromPet)
                .WithMany()
                .HasForeignKey(x => x.FromPetId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.ToPet)
                .WithMany()
                .HasForeignKey(x => x.ToPetId)
                .OnDelete(DeleteBehavior.Restrict);

            e.Property(x => x.Status).HasConversion<int>();
        });

        modelBuilder.Entity<Playdate>(e =>
        {
            e.HasKey(x => x.Id);

            e.HasOne(x => x.PetA)
                .WithMany()
                .HasForeignKey(x => x.PetAId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.PetB)
                .WithMany()
                .HasForeignKey(x => x.PetBId)
                .OnDelete(DeleteBehavior.Restrict);

            e.Property(x => x.LocationName).HasMaxLength(200);
        });

        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
    }
}