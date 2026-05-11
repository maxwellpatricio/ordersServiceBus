using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Orders.Api.BackgroundServices;
using Orders.Api.Data;
using Orders.Api.HealthChecks;
using Orders.Api.Hubs;
using Orders.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["DATABASE_URL"]
    ?? throw new InvalidOperationException("Database connection string not configured.");

builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseNpgsql(connectionString));

// Azure Service Bus
var sbConnectionString = builder.Configuration["ServiceBus:ConnectionString"]
    ?? builder.Configuration["SERVICEBUS_CONNECTION_STRING"]
    ?? throw new InvalidOperationException("Service Bus connection string not configured.");

builder.Services.AddSingleton(new ServiceBusClient(sbConnectionString));
builder.Services.AddScoped<ServiceBusPublisher>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<LogService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<PerplexityService>();
builder.Services.AddHostedService<OutboxProcessor>();
builder.Services.AddSignalR();
builder.Services.AddTransient<ServiceBusHealthCheck>();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(name: "database")
    .AddCheck<ServiceBusHealthCheck>("servicebus");
builder.Services.AddCors(opts => opts.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.MapControllers();
app.MapHub<OrdersHub>("/hubs/orders");

app.MapHealthChecks("/health", new HealthCheckOptions { ResponseWriter = HealthCheckResponseWriter.WriteAsync });
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = HealthCheckResponseWriter.WriteAsync
});

await app.RunAsync();
