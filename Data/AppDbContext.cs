using Microsoft.EntityFrameworkCore;
using P4_Backend_Car_App.Types;

using P4_Backend_Car_App.Models;

namespace P4_Backend_Car_App.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<EngineCapacity> EngineCapacities { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.NormalizedEmail).IsUnique();
                entity.HasIndex(u => u.NormalizedUsername).IsUnique();

                entity.Property(u => u.NormalizedEmail).IsRequired().HasMaxLength(100);
                entity.Property(u => u.NormalizedUsername).IsRequired().HasMaxLength(50);

                
                entity.HasQueryFilter(u => u.IsActive);
            });

            modelBuilder.Entity<Manufacturer>(entity =>
            {
                entity.HasIndex(m => m.NormalizedName).IsUnique();

                entity.Property(m => m.NormalizedName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasQueryFilter(m => m.IsActive);
            });

            modelBuilder.Entity<EngineCapacity>(entity =>
            {
                entity.HasIndex(e => e.NormalizedName).IsUnique();

                entity.Property(e => e.NormalizedName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasQueryFilter(e => e.IsActive);
            });

            modelBuilder.Entity<Car>(entity =>
            {
                entity.Property(c => c.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(c => c.Price)
                    .HasColumnType("decimal(18,2)");

                entity.HasQueryFilter(c => c.IsActive);

                entity.Property(c => c.FuelType)
                    .HasConversion<string>();

                entity.Property(c => c.Transmission)
                    .HasConversion<string>();

                // Relationships
                entity.HasOne(c => c.Manufacturer)
                    .WithMany(m => m.Cars)
                    .HasForeignKey(c => c.ManufacturerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.EngineCapacity)
                    .WithMany(e => e.Cars)
                    .HasForeignKey(c => c.EngineCapacityId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<User>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;

                    // Prevent CreatedAt from being overwritten
                    entry.Property(x => x.CreatedAt).IsModified = false;
                }
            }

            foreach (var entry in ChangeTracker.Entries<Manufacturer>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.NormalizedName = entry.Entity.Name.ToUpper() ?? string.Empty;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.NormalizedName = entry.Entity.Name?.ToUpper() ?? string.Empty;
                }
            }

            foreach (var entry in ChangeTracker.Entries<EngineCapacity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.NormalizedName =
                        entry.Entity.Name?.ToUpper() ?? string.Empty;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.NormalizedName =
                        entry.Entity.Name?.ToUpper() ?? string.Empty;
                }
            }

            foreach (var entry in ChangeTracker.Entries<Car>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }


            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
