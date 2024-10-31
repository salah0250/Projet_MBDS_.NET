using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;
using Gauniv.WebServer.Data;


namespace Gauniv.WebServer.Websocket
{
    public class OnlineStatus
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Count { get; set; }
    }

    public class OnlineHub : Hub
    {
        private static readonly Dictionary<string, OnlineStatus> _connectedUsers = new();
        private static readonly object _lock = new();
        private readonly UserManager<User> _userManager;
        private readonly IHubContext<OnlineHub> _hubContext;

        public OnlineHub(
            UserManager<User> userManager,
            IHubContext<OnlineHub> hubContext)
        {
            _userManager = userManager;
            _hubContext = hubContext;
        }

        // Propriété statique publique pour accéder aux utilisateurs connectés
        public static IReadOnlyDictionary<string, OnlineStatus> ConnectedUsers
        {
            get
            {
                lock (_lock)
                {
                    return new Dictionary<string, OnlineStatus>(_connectedUsers);
                }
            }
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId)) return;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return;

            lock (_lock)
            {
                if (_connectedUsers.ContainsKey(userId))
                {
                    _connectedUsers[userId].Count++;
                }
                else
                {
                    _connectedUsers[userId] = new OnlineStatus
                    {
                        UserId = userId,
                        UserName = user.UserName,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Count = 1
                    };
                }
            }

            await BroadcastOnlineUsers();
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId)) return;

            lock (_lock)
            {
                if (_connectedUsers.ContainsKey(userId))
                {
                    _connectedUsers[userId].Count--;

                    if (_connectedUsers[userId].Count <= 0)
                    {
                        _connectedUsers.Remove(userId);
                    }
                }
            }

            await BroadcastOnlineUsers();
            await base.OnDisconnectedAsync(exception);
        }

        private async Task BroadcastOnlineUsers()
        {
            Dictionary<string, OnlineStatus> currentUsers;
            lock (_lock)
            {
                currentUsers = new Dictionary<string, OnlineStatus>(_connectedUsers);
            }

            await _hubContext.Clients.All.SendAsync("ReceiveOnlineUsers", currentUsers);
        }
    }
}