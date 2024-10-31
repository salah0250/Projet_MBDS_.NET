using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Gauniv.WebServer.Data;
using Gauniv.WebServer.Websocket;
using Microsoft.EntityFrameworkCore;
using Gauniv.WebServer.Services;

namespace Gauniv.WebServer.Controllers
{
    public class PlayersController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly FriendService _friendService;


        public PlayersController(
       UserManager<User> userManager,
       FriendService friendService)
        {
            _userManager = userManager;
            _friendService = friendService;
        }

        public async Task<IActionResult> Index()
        {
            var connectedUsers = OnlineHub.ConnectedUsers;

            var users = await _userManager.Users
                .Select(u => new PlayerViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    IsOnline = connectedUsers.ContainsKey(u.Id),
                    ConnectionCount = connectedUsers.ContainsKey(u.Id)
                        ? connectedUsers[u.Id].Count
                        : 0
                })
                .ToListAsync();

            return View(users);
        }
        public IActionResult Amis()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetFriends()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Unauthorized();

            var friends = await _friendService.GetFriendsAsync(userId);
            return Json(friends);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFriend([FromBody] AddFriendModel model)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Unauthorized();

            var success = await _friendService.AddFriendAsync(userId, model.FriendId);
            return success ? Ok() : BadRequest("Unable to add friend");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFriend([FromBody] RemoveFriendModel model)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Unauthorized();

            var success = await _friendService.RemoveFriendAsync(userId, model.FriendId);
            return success ? Ok() : BadRequest("Unable to remove friend");
        }
    }

    public class PlayerViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsOnline { get; set; }
        public int ConnectionCount { get; set; }
    }
    public class AddFriendModel
    {
        public string FriendId { get; set; }
    }

    public class RemoveFriendModel
    {
        public string FriendId { get; set; }
    }
}