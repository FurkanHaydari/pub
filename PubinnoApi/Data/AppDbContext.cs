using Microsoft.EntityFrameworkCore;

namespace PubinnoApi.Data;

public class AppDbContext : DbContext
{
    public DbSet<Pour> Pours => Set<Pour>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Pour>(entity =>
        {
            entity.HasKey(e => e.EventId);

            // Indeksler testlerdeki okuma performansini artirmak icin onemli
            entity.HasIndex(e => e.DeviceId);
            entity.HasIndex(e => e.StartedAt);
        });
    }
}
