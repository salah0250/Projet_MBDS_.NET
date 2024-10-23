using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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


        public HomeController(ILogger<HomeController> logger, ApplicationDbContext applicationDbContext, UserManager<User> userManager)
        {
            _logger = logger;
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var games = _applicationDbContext.Games.ToList();  // Vérifiez que vous récupérez bien les jeux
            return View(games);  // Passez la liste des jeux à la vue
        }

        // Only allow admins to access the Create method
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();  // Cette méthode retourne une vue HTML pour ajouter un jeu
        }

        [HttpPost]
        [Authorize(Roles = "Admin")] // Ensure only Admins can post to Create
        public async Task<IActionResult> Create(Game game)
        {
            if (ModelState.IsValid)
            {
                // Convertir toutes les propriétés DateTime à UTC
                game.ReleaseDate = game.ReleaseDate.ToUniversalTime(); // Ajoutez cette ligne si vous avez une propriété ReleaseDate
                game.CreatedAt = DateTime.UtcNow; 

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

        // Ensure only Admins can post to Edit
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Game game)
        {
            if (ModelState.IsValid)
            {
                // Convertir toutes les propriétés DateTime à UTC 
                game.ReleaseDate = game.ReleaseDate.ToUniversalTime(); // Ajoutez cette ligne si vous avez une propriété ReleaseDate
                game.CreatedAt = DateTime.UtcNow; 
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
    }
}
