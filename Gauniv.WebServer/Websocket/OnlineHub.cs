using Gauniv.WebServer.Data;
using Gauniv.WebServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

public class OnlineStatus
{
    public User User { get; set; }
    public int Count { get; set; }
}

namespace Gauniv.WebServer.Websocket
{
    public class OnlineHub : Hub
    {
        public static Dictionary<string, OnlineStatus> ConnectedUsers = new();
        private readonly UserManager<User> userManager;
        private readonly RedisService redisService;

        public OnlineHub(UserManager<User> userManager, RedisService redisService)
        {
            this.userManager = userManager;
            this.redisService = redisService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier; // Assurez-vous que l'ID utilisateur est correctement configuré
            var user = await userManager.FindByIdAsync(userId); // Récupération de l'utilisateur

            if (user != null)
            {
                // Mettre à jour le statut en ligne
                if (ConnectedUsers.ContainsKey(userId))
                {
                    ConnectedUsers[userId].Count++;
                }
                else
                {
                    ConnectedUsers[userId] = new OnlineStatus { User = user, Count = 1 };
                }

                // Envoyer la liste mise à jour des utilisateurs en ligne
                await SendOnlineUsers();
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier; // Assurez-vous que l'ID utilisateur est correctement configur

            if (ConnectedUsers.ContainsKey(userId))
            {
                ConnectedUsers[userId].Count--;

                // Supprimer l'utilisateur si le compteur atteint zéro
                if (ConnectedUsers[userId].Count <= 0)
                {
                    ConnectedUsers.Remove(userId);
                }

                // Envoyer la liste mise à jour des utilisateurs en ligne
                await SendOnlineUsers();
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendOnlineUsers()
        {
            await Clients.All.SendAsync("ReceiveOnlineUsers", ConnectedUsers);
        }
    }
}
