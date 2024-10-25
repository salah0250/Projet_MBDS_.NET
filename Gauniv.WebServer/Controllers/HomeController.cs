using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using CommunityToolkit.HighPerformance;
using Gauniv.WebServer.Data;
using Gauniv.WebServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using X.PagedList.Extensions;

namespace Gauniv.WebServer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;


        public HomeController(ILogger<HomeController> logger, ApplicationDbContext applicationDbContext, UserManager<User> userManager, ApplicationDbContext context)
        {
            _logger = logger;
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index(string searchTerm, decimal? minPrice, decimal? maxPrice, Category? category)
        {
            // Inclure la relation UserGames pour récupérer les utilisateurs qui possèdent chaque jeu
            var games = _applicationDbContext.Games
                        .Include(g => g.UserGames) // Ajout du Include pour inclure la relation avec UserGames
                        .AsQueryable();

            // Filtrer par nom (Title) - recherche insensible à la casse
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower().Trim();
                games = games.Where(g => g.Title.ToLower().Contains(searchTerm) ||
                                        (g.Description != null && g.Description.ToLower().Contains(searchTerm)));
            }

            // Filtrer par prix minimum
            if (minPrice.HasValue)
            {
                games = games.Where(g => g.Price >= minPrice.Value);
            }

            // Filtrer par prix maximum
            if (maxPrice.HasValue)
            {
                games = games.Where(g => g.Price <= maxPrice.Value);
            }

            // Filtrer par catégorie
            if (category.HasValue)
            {
                games = games.Where(g => g.Category == category.Value);
            }

            return View(games.ToList());
        }

       

        // Only allow admins to access the Create method
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();  // Cette méthode retourne une vue HTML pour ajouter un jeu
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Game game, IFormFile file, IFormFile imageFile)
        {
            // Disable validation for Payload and Image fields
            ModelState.Remove("Payload");
            ModelState.Remove("Image");

            if (ModelState.IsValid)
            {
                game.ReleaseDate = game.ReleaseDate.ToUniversalTime();
                game.CreatedAt = DateTime.UtcNow;

                // Handle Payload file
                if (file != null && file.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(memoryStream);
                        game.Payload = memoryStream.ToArray();
                    }
                }

                // Handle Image file
                if (imageFile != null && imageFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await imageFile.CopyToAsync(memoryStream);
                        game.Image = memoryStream.ToArray(); // Store the image
                    }
                }

                _applicationDbContext.Games.Add(game);
                await _applicationDbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(game);
        }



        // Only allow admins to access the Edit method
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var game = await _applicationDbContext.Games.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }
            return View(game);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Game game, int id, IFormFile imageFile)
        {
            // Disable validation for Payload and Image fields
            ModelState.Remove("Payload");
            ModelState.Remove("Image");

            if (ModelState.IsValid)
            {
                game.ReleaseDate = game.ReleaseDate.ToUniversalTime();
                game.CreatedAt = DateTime.UtcNow;

                var existingGame = await _applicationDbContext.Games.FindAsync(id);
                if (existingGame != null)
                {
                    _applicationDbContext.Entry(existingGame).State = EntityState.Detached;
                }
                game.Payload = existingGame.Payload;

                // Handle new image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await imageFile.CopyToAsync(memoryStream);
                        game.Image = memoryStream.ToArray();
                    }
                }
                else
                {
                    game.Image = existingGame.Image; // Retain the old image if no new image is uploaded
                }

                _applicationDbContext.Update(game);
                await _applicationDbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(game);
        }


        // Only allow admins to access the Delete method
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var game = await _applicationDbContext.Games.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }
            return View(game);
        }

        // Ensure only Admins can post to DeleteConfirmed
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var game = await _applicationDbContext.Games.FindAsync(id);
            if (game != null)
            {
                _applicationDbContext.Games.Remove(game);
                await _applicationDbContext.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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
