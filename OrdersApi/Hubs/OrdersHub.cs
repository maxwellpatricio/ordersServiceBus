using Microsoft.AspNetCore.SignalR;

namespace Orders.Api.Hubs;

public class OrdersHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }
}
