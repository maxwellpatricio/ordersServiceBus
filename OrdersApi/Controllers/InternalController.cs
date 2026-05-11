using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Orders.Api.Data;
using Orders.Api.Hubs;

namespace Orders.Api.Controllers;

[ApiController]
[Route("internal")]
public class InternalController(AppDbContext db, IHubContext<OrdersHub> hub) : ControllerBase
{
    [HttpPost("orders/{id:guid}/notify")]
    public async Task<IActionResult> Notify(Guid id)
    {
        var order = await db.Orders.FindAsync(id);
        if (order is null) return NotFound();

        await hub.Clients.All.SendAsync("OrderStatusUpdated", new
        {
            id = order.Id,
            cliente = order.Cliente,
            produto = order.Produto,
            valor = order.Valor,
            status = order.Status.ToString(),
            dataCriacao = order.DataCriacao
        });

        return Ok();
    }
}
