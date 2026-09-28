using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Gym.Infrastructure.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            Console.WriteLine($"Connected User: {userId}");
            Console.WriteLine($"SignalR UserIdentifier: {Context.UserIdentifier}");

            await base.OnConnectedAsync();
        }
    }
}
