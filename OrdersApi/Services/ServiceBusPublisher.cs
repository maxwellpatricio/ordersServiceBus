using Azure.Messaging.ServiceBus;

namespace Orders.Api.Services;

public class ServiceBusPublisher(ServiceBusClient client, IConfiguration config, ILogger<ServiceBusPublisher> logger)
{
    public async Task PublishAsync(string payload, string eventType, string correlationId)
    {
        var queueName = config["ServiceBus:QueueName"] ?? "orders-queue";
        var sender = client.CreateSender(queueName);

        var message = new ServiceBusMessage(payload)
        {
            CorrelationId = correlationId,
            ApplicationProperties = { ["EventType"] = eventType }
        };

        try
        {
            await sender.SendMessageAsync(message);
            logger.LogInformation("Published {EventType} message for {CorrelationId}", eventType, correlationId);
        }
        finally
        {
            await sender.DisposeAsync();
        }
    }
}
