using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Orders.Api.HealthChecks;

public class ServiceBusHealthCheck(ServiceBusClient client, IConfiguration config) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        try
        {
            var queueName = config["ServiceBus:QueueName"] ?? "orders-queue";
            var receiver = client.CreateReceiver(queueName);
            await receiver.PeekMessageAsync(cancellationToken: ct);
            await receiver.DisposeAsync();
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded(ex.Message);
        }
    }
}
