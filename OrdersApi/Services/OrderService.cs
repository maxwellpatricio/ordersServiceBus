using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Orders.Api.Data;
using Orders.Api.Models;

namespace Orders.Api.Services;

public record CreateOrderRequest(string Cliente, string Produto, decimal Valor);

public class OrderService(AppDbContext db, LogService logService)
{
    public async Task<Order> CreateAsync(CreateOrderRequest req)
    {
        var order = new Order
        {
            Cliente = req.Cliente,
            Produto = req.Produto,
            Valor = req.Valor
        };

        var outbox = new OutboxMessage
        {
            OrderId = order.Id,
            EventType = "OrderCreated",
            Payload = JsonSerializer.Serialize(new { orderId = order.Id, timestamp = order.DataCriacao })
        };

        var history = new OrderStatusHistory
        {
            OrderId = order.Id,
            FromStatus = null,
            ToStatus = OrderStatus.Pendente
        };

        await using var tx = await db.Database.BeginTransactionAsync();
        db.Orders.Add(order);
        db.OutboxMessages.Add(outbox);
        db.OrderStatusHistories.Add(history);
        await db.SaveChangesAsync();
        await tx.CommitAsync();

        await logService.InfoAsync($"Pedido criado: Cliente={order.Cliente}, Produto={order.Produto}, Valor={order.Valor:C}", order.Id);

        return order;
    }

    public async Task<List<Order>> GetAllAsync() =>
        await db.Orders.OrderByDescending(o => o.DataCriacao).ToListAsync();

    public async Task<Order?> GetByIdAsync(Guid id) =>
        await db.Orders
            .Include(o => o.StatusHistory.OrderBy(h => h.ChangedAt))
            .FirstOrDefaultAsync(o => o.Id == id);
}
