using Gauniv.WebServer.Data;
using Gauniv.WebServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

public class OnlineStatus()
{
    public User User { get; set; }
    public int Count { get; set; }
}

namespace Gauniv.WebServer.Websocket
{
    public class OnlineHub : Hub
    {

        public static Dictionary<string, OnlineStatus> ConnectedUsers = [];
        private readonly UserManager<User> userManager;
        private readonly RedisService redisService;

        public OnlineHub(UserManager<User> userManager, RedisService redisService)
        {
            this.userManager = userManager;
            this.redisService = redisService;
        }

        public async override Task OnConnectedAsync()
        {
        }

        public async override Task OnDisconnectedAsync(Exception? exception)
        {
        }
    }
}
