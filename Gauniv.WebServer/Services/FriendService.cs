using Gauniv.WebServer.Data;
using Gauniv.WebServer.Models;
using Gauniv.WebServer.Websocket;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Gauniv.WebServer.Services
{
    public class FriendService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<OnlineHub> _hubContext;

        public FriendService(ApplicationDbContext context, IHubContext<OnlineHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<List<FriendViewModel>> GetFriendsAsync(string userId)
        {
            var friendships = await _context.Friendships
                .Where(f => f.RequesterId == userId || f.AddresseeId == userId)
                .Include(f => f.Requester)
                .Include(f => f.Addressee)
                .ToListAsync();

            var friendViewModels = friendships.Select(f =>
            {
                var friend = f.RequesterId == userId ? f.Addressee : f.Requester;
                return new FriendViewModel
                {
                    Id = friend.Id,
                    UserName = friend.UserName,
                    FirstName = friend.FirstName,
                    LastName = friend.LastName,
                    IsOnline = OnlineHub.ConnectedUsers.ContainsKey(friend.Id),
                    ConnectionCount = OnlineHub.ConnectedUsers.ContainsKey(friend.Id)
                        ? OnlineHub.ConnectedUsers[friend.Id].Count
                        : 0
                };
            }).ToList();

            return friendViewModels;
        }

        public async Task<bool> AddFriendAsync(string requesterId, string addresseeId)
        {
            if (requesterId == addresseeId)
                return false;

            var existingFriendship = await _context.Friendships
                .AnyAsync(f =>
                    (f.RequesterId == requesterId && f.AddresseeId == addresseeId) ||
                    (f.RequesterId == addresseeId && f.AddresseeId == requesterId));

            if (existingFriendship)
                return false;

            var friendship = new Friendship
            {
                RequesterId = requesterId,
                AddresseeId = addresseeId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Friendships.Add(friendship);
            await _context.SaveChangesAsync();

            await NotifyFriendshipChange(requesterId);
            await NotifyFriendshipChange(addresseeId);

            return true;
        }

        public async Task<bool> RemoveFriendAsync(string userId, string friendId)
        {
            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f =>
                    (f.RequesterId == userId && f.AddresseeId == friendId) ||
                    (f.RequesterId == friendId && f.AddresseeId == userId));

            if (friendship == null)
                return false;

            _context.Friendships.Remove(friendship);
            await _context.SaveChangesAsync();

            await NotifyFriendshipChange(userId);
            await NotifyFriendshipChange(friendId);

            return true;
        }

        private async Task NotifyFriendshipChange(string userId)
        {
            var friends = await GetFriendsAsync(userId);
            await _hubContext.Clients.User(userId).SendAsync("FriendsListUpdated", friends);
        }
    }

}
