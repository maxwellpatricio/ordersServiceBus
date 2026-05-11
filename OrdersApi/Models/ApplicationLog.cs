namespace Orders.Api.Models;

public class ApplicationLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Level { get; set; } = "Info";    // Info | Warning | Error
    public string Source { get; set; } = "API";    // API | Worker
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public Guid? OrderId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
