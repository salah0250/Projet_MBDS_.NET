using Gauniv.WebServer.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Gauniv.WebServer.Controllers
{
    public class GameController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public GameController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize]
        public async Task<IActionResult> Purchase(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (_context.UserGames.Any(ug => ug.UserId == userId && ug.GameId == id))
            {
                // L'utilisateur possède déjà le jeu
                return RedirectToAction("Library");
            }

            var userGame = new UserGame
            {
                UserId = userId,
                GameId = id
            };

            _context.UserGames.Add(userGame);
            await _context.SaveChangesAsync();

            return RedirectToAction("Library"); // Redirige vers la bibliothèque après l'achat
        }

        [Authorize]
        public async Task<IActionResult> Download(int id)
        {
            var userId = _userManager.GetUserId(User);
            var userGame = _context.UserGames.FirstOrDefault(ug => ug.UserId == userId && ug.GameId == id);

            if (userGame == null)
            {
                return Unauthorized(); // L'utilisateur n'a pas acheté le jeu
            }

            var game = await _context.Games.FindAsync(id);
            if (game?.Payload == null) return NotFound();

            return File(game.Payload, "application/octet-stream", $"{game.Title}.zip");
        }


        [Authorize]
        public IActionResult Library()
        {
            var userId = _userManager.GetUserId(User);
            var userGames = _context.UserGames
                .Where(ug => ug.UserId == userId)
                .Select(ug => ug.Game)
                .ToList();

            return View(userGames);
        }

    }

}
