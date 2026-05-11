using Microsoft.EntityFrameworkCore;
using Orders.Api.Models;

namespace Orders.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<ApplicationLog> ApplicationLogs => Set<ApplicationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.Cliente).IsRequired().HasMaxLength(200);
            e.Property(o => o.Produto).IsRequired().HasMaxLength(200);
            e.Property(o => o.Valor).HasPrecision(18, 2);
            e.Property(o => o.Status).HasConversion<string>();
            e.HasMany(o => o.StatusHistory).WithOne(h => h.Order).HasForeignKey(h => h.OrderId);
            e.HasMany(o => o.OutboxMessages).WithOne(m => m.Order).HasForeignKey(m => m.OrderId);
        });

        modelBuilder.Entity<OrderStatusHistory>(e =>
        {
            e.HasKey(h => h.Id);
            e.Property(h => h.FromStatus).HasConversion<string>();
            e.Property(h => h.ToStatus).HasConversion<string>();
        });

        modelBuilder.Entity<OutboxMessage>(e =>
        {
            e.HasKey(m => m.Id);
            e.HasIndex(m => m.ProcessedAt);
        });

        modelBuilder.Entity<ApplicationLog>(e =>
        {
            e.HasKey(l => l.Id);
            e.HasIndex(l => l.CreatedAt);
            e.Property(l => l.Level).HasMaxLength(20);
            e.Property(l => l.Source).HasMaxLength(20);
        });
    }
}
