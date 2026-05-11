using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Orders.Worker.Data;

namespace Orders.Worker.Workers;

public class OrderProcessingWorker(
    ServiceBusClient sbClient,
    IServiceScopeFactory scopeFactory,
    IHttpClientFactory httpClientFactory,
    IConfiguration config,
    ILogger<OrderProcessingWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueName = config["ServiceBus:QueueName"] ?? "orders-queue";
        var apiUrl = config["Api:InternalUrl"] ?? "http://api:8080";

        var processor = sbClient.CreateProcessor(queueName, new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 1,
            AutoCompleteMessages = false
        });

        processor.ProcessMessageAsync += msg => ProcessMessageAsync(msg, apiUrl, stoppingToken);
        processor.ProcessErrorAsync += args =>
        {
            logger.LogError(args.Exception, "Service Bus error: {Source}", args.ErrorSource);
            return Task.CompletedTask;
        };

        await processor.StartProcessingAsync(stoppingToken);
        await Task.Delay(Timeout.Infinite, stoppingToken);
        await processor.StopProcessingAsync();
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args, string apiUrl, CancellationToken ct)
    {
        var body = args.Message.Body.ToString();
        var payload = JsonDocument.Parse(body).RootElement;

        if (!payload.TryGetProperty("orderId", out var orderIdEl) || !Guid.TryParse(orderIdEl.GetString(), out var orderId))
        {
            logger.LogWarning("Invalid message payload, completing without processing.");
            await args.CompleteMessageAsync(args.Message, ct);
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WorkerDbContext>();

        var order = await db.Orders.FindAsync([orderId], ct);
        if (order is null)
        {
            logger.LogWarning("Order {OrderId} not found, skipping.", orderId);
            await WriteLogAsync(db, "Warning", $"Pedido {orderId} não encontrado na fila — mensagem descartada.", orderId, ct);
            await args.CompleteMessageAsync(args.Message, ct);
            return;
        }

        if (order.Status != OrderStatus.Pendente)
        {
            logger.LogInformation("Order {OrderId} already in status {Status}, skipping.", orderId, order.Status);
            await WriteLogAsync(db, "Info", $"Pedido {orderId} já está em '{order.Status}' — processamento ignorado (idempotência).", orderId, ct);
            await args.CompleteMessageAsync(args.Message, ct);
            return;
        }

        // Pendente → Processando
        await UpdateStatusAsync(db, order, OrderStatus.Processando, ct);
        await WriteLogAsync(db, "Info", $"Pedido {orderId} → Processando.", orderId, ct);
        await NotifyApiAsync(apiUrl, orderId, ct);

        logger.LogInformation("Order {OrderId} → Processando. Waiting 5s...", orderId);
        await Task.Delay(5000, ct);

        // Processando → Finalizado
        await UpdateStatusAsync(db, order, OrderStatus.Finalizado, ct);
        await WriteLogAsync(db, "Info", $"Pedido {orderId} → Finalizado.", orderId, ct);
        await NotifyApiAsync(apiUrl, orderId, ct);

        logger.LogInformation("Order {OrderId} → Finalizado.", orderId);
        await args.CompleteMessageAsync(args.Message, ct);
    }

    private static async Task WriteLogAsync(WorkerDbContext db, string level, string message, Guid? orderId, CancellationToken ct)
    {
        db.ApplicationLogs.Add(new ApplicationLog { Level = level, Message = message, OrderId = orderId });
        await db.SaveChangesAsync(ct);
    }

    private static async Task UpdateStatusAsync(WorkerDbContext db, Order order, OrderStatus newStatus, CancellationToken ct)
    {
        var history = new OrderStatusHistory
        {
            OrderId = order.Id,
            FromStatus = order.Status,
            ToStatus = newStatus
        };

        order.Status = newStatus;
        db.OrderStatusHistories.Add(history);
        await db.SaveChangesAsync(ct);
    }

    private async Task NotifyApiAsync(string apiUrl, Guid orderId, CancellationToken ct)
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            await client.PostAsync($"{apiUrl}/internal/orders/{orderId}/notify", null, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to notify API for order {OrderId} — SignalR update skipped.", orderId);
        }
    }
}
