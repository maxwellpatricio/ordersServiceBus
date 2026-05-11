using Microsoft.EntityFrameworkCore;
using Orders.Api.Data;
using Orders.Api.Services;

namespace Orders.Api.BackgroundServices;

public class OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessPendingMessagesAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task ProcessPendingMessagesAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<ServiceBusPublisher>();

        var pending = await db.OutboxMessages
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.CreatedAt)
            .Take(10)
            .ToListAsync(ct);

        foreach (var msg in pending)
        {
            try
            {
                await publisher.PublishAsync(msg.Payload, msg.EventType, msg.OrderId.ToString());
                msg.ProcessedAt = DateTime.UtcNow;
                await db.SaveChangesAsync(ct);
                logger.LogInformation("Outbox: published message {Id} for order {OrderId}", msg.Id, msg.OrderId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox: failed to publish message {Id}", msg.Id);
            }
        }
    }
}
