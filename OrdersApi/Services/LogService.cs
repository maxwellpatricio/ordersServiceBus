using Orders.Api.Data;
using Orders.Api.Models;

namespace Orders.Api.Services;

public class LogService(AppDbContext db)
{
    public Task InfoAsync(string message, Guid? orderId = null, string? details = null)
        => WriteAsync("Info", "API", message, orderId, details);

    public Task WarningAsync(string message, Guid? orderId = null, string? details = null)
        => WriteAsync("Warning", "API", message, orderId, details);

    public Task ErrorAsync(string message, Guid? orderId = null, string? details = null)
        => WriteAsync("Error", "API", message, orderId, details);

    private async Task WriteAsync(string level, string source, string message, Guid? orderId, string? details)
    {
        db.ApplicationLogs.Add(new ApplicationLog
        {
            Level = level,
            Source = source,
            Message = message,
            OrderId = orderId,
            Details = details
        });
        await db.SaveChangesAsync();
    }
}
