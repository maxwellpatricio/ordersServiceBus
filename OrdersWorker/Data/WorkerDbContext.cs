using Microsoft.EntityFrameworkCore;

namespace Orders.Worker.Data;

public enum OrderStatus { Pendente, Processando, Finalizado }

public class Order
{
    public Guid Id { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string Produto { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime DataCriacao { get; set; }
}

public class OrderStatusHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public OrderStatus? FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}

public class ApplicationLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Level { get; set; } = "Info";
    public string Source { get; set; } = "Worker";
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public Guid? OrderId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class WorkerDbContext(DbContextOptions<WorkerDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<ApplicationLog> ApplicationLogs => Set<ApplicationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(e =>
        {
            e.ToTable("Orders");
            e.HasKey(o => o.Id);
            e.Property(o => o.Status).HasConversion<string>();
        });

        modelBuilder.Entity<OrderStatusHistory>(e =>
        {
            e.ToTable("OrderStatusHistories");
            e.HasKey(h => h.Id);
            e.Property(h => h.FromStatus).HasConversion<string>();
            e.Property(h => h.ToStatus).HasConversion<string>();
        });

        modelBuilder.Entity<ApplicationLog>(e =>
        {
            e.ToTable("ApplicationLogs");
            e.HasKey(l => l.Id);
        });
    }
}
