using Microsoft.AspNetCore.Mvc;
using Orders.Api.Models;
using Orders.Api.Services;

namespace Orders.Api.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController(OrderService orderService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Cliente) || string.IsNullOrWhiteSpace(req.Produto) || req.Valor <= 0)
            return BadRequest(new { error = "Cliente, Produto e Valor são obrigatórios. Valor deve ser maior que zero." });

        var order = await orderService.CreateAsync(req);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, ToResponse(order));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await orderService.GetAllAsync();
        return Ok(orders.Select(o => ToResponse(o)));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var order = await orderService.GetByIdAsync(id);
        return order is null ? NotFound() : Ok(ToDetailResponse(order));
    }

    private static object ToResponse(Order o) => new
    {
        id = o.Id,
        cliente = o.Cliente,
        produto = o.Produto,
        valor = o.Valor,
        status = o.Status.ToString(),
        dataCriacao = o.DataCriacao
    };

    private static object ToDetailResponse(Order o) => new
    {
        id = o.Id,
        cliente = o.Cliente,
        produto = o.Produto,
        valor = o.Valor,
        status = o.Status.ToString(),
        dataCriacao = o.DataCriacao,
        statusHistory = o.StatusHistory.Select(h => new
        {
            fromStatus = h.FromStatus?.ToString(),
            toStatus = h.ToStatus.ToString(),
            changedAt = h.ChangedAt
        })
    };
}
