using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orders.Api.Data;

namespace Orders.Api.Controllers;

[ApiController]
[Route("logs")]
public class LogsController(AppDbContext db) : ControllerBase
{
    [HttpGet("app")]
    public async Task<IActionResult> GetAppLogs()
    {
        var logs = await db.ApplicationLogs
            .OrderByDescending(l => l.CreatedAt)
            .Take(200)
            .Select(l => new
            {
                l.Id,
                l.Level,
                l.Source,
                l.Message,
                l.Details,
                l.OrderId,
                l.CreatedAt
            })
            .ToListAsync();

        return Ok(logs);
    }

    [HttpGet("status-history")]
    public async Task<IActionResult> GetStatusHistory()
    {
        var history = await db.OrderStatusHistories
            .OrderByDescending(h => h.ChangedAt)
            .Take(200)
            .Select(h => new
            {
                h.Id,
                h.OrderId,
                cliente = h.Order.Cliente,
                fromStatus = h.FromStatus != null ? h.FromStatus.ToString() : null,
                toStatus = h.ToStatus.ToString(),
                h.ChangedAt
            })
            .ToListAsync();

        return Ok(history);
    }

    [HttpGet("outbox")]
    public async Task<IActionResult> GetOutboxMessages()
    {
        var messages = await db.OutboxMessages
            .OrderByDescending(m => m.CreatedAt)
            .Take(200)
            .Select(m => new
            {
                m.Id,
                m.OrderId,
                cliente = m.Order.Cliente,
                m.EventType,
                m.CreatedAt,
                m.ProcessedAt,
                processed = m.ProcessedAt != null
            })
            .ToListAsync();

        return Ok(messages);
    }
}
