using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Orders.Worker.Data;
using Orders.Worker.Workers;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["DATABASE_URL"]
    ?? throw new InvalidOperationException("Database connection string not configured.");

builder.Services.AddDbContext<WorkerDbContext>(opts => opts.UseNpgsql(connectionString));

var sbConnectionString = builder.Configuration["ServiceBus:ConnectionString"]
    ?? builder.Configuration["SERVICEBUS_CONNECTION_STRING"]
    ?? throw new InvalidOperationException("Service Bus connection string not configured.");

builder.Services.AddSingleton(new ServiceBusClient(sbConnectionString));
builder.Services.AddHttpClient();
builder.Services.AddHostedService<OrderProcessingWorker>();

var host = builder.Build();
await host.RunAsync();
