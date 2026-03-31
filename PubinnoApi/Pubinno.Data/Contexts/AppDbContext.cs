using Microsoft.EntityFrameworkCore;
using Pubinno.Data.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Pubinno.Data.Contexts;

public class AppDbContext : DbContext
{
    public DbSet<Pour> Pours => Set<Pour>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Location> Locations => Set<Location>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Pours Table Config
        modelBuilder.Entity<Pour>(entity =>
        {
            entity.HasKey(e => e.EventId);

            entity.HasIndex(e => e.DeviceId);
            entity.HasIndex(e => e.StartedAt);
            
            // Foreign Keys
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId);
                  
            entity.HasOne(e => e.Location)
                  .WithMany()
                  .HasForeignKey(e => e.LocationId);
        });

        // Seed Data
        var products = new[] { "guinness", "ipa", "lager", "pilsner", "stout", "efes-pilsen", "efes-malt", "bomonti-filtresiz", "tuborg-gold", "tuborg-amber" };
        var locations = new[] { "istanbul-kadikoy-01", "istanbul-besiktas-01", "izmir-alsancak-01", "ankara-cankaya-01", "london-soho-01" };

        modelBuilder.Entity<Product>().HasData(
            products.Select(p => new Product { Id = p }).ToArray()
        );

        modelBuilder.Entity<Location>().HasData(
            locations.Select(l => new Location { Id = l }).ToArray()
        );
    }
}
