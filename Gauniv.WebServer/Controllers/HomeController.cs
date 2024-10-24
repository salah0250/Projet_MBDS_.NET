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
            var games = _applicationDbContext.Games.ToList();  // V�rifiez que vous r�cup�rez bien les jeux
            return View(games);  // Passez la liste des jeux � la vue
        }

        // Only allow admins to access the Create method
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();  // Cette m�thode retourne une vue HTML pour ajouter un jeu
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Game game, IFormFile file)
        {
            // D�sactiver la validation du champ Payload
            ModelState.Remove("Payload");

            if (ModelState.IsValid)
            {
                // Convertir les propri�t�s DateTime � UTC
                game.ReleaseDate = game.ReleaseDate.ToUniversalTime();
                game.CreatedAt = DateTime.UtcNow;

                // Si un fichier a �t� upload�, on le convertit en tableau de bytes
                if (file != null && file.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(memoryStream);
                        game.Payload = memoryStream.ToArray(); // Stocker le fichier dans la propri�t� Payload
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
        public async Task<IActionResult> Edit(Game game, int id)
        {
            // D�sactiver la validation du champ Payload
            ModelState.Remove("Payload");

            if (ModelState.IsValid)
            {
                // Convertir toutes les propri�t�s DateTime � UTC 
                game.ReleaseDate = game.ReleaseDate.ToUniversalTime(); // Ajoutez cette ligne si vous avez une propri�t� ReleaseDate
                game.CreatedAt = DateTime.UtcNow;
                // R�cup�rer le jeu actuel depuis la base de donn�es
                var existingGame = await _applicationDbContext.Games.FindAsync(id);
                if (existingGame != null)
                {
                    _applicationDbContext.Entry(existingGame).State = EntityState.Detached;
                }
                game.Payload = existingGame.Payload;
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


        // Action pour t�l�charger un jeu
        public async Task<IActionResult> Download(int id)
        {
            var game = await _applicationDbContext.Games.FindAsync(id);

            if (game == null || game.Payload == null)
            {
                return NotFound();
            }

            // T�l�charger le fichier ZIP
            return File(game.Payload, "application/zip", $"{game.Title}.zip");
        }
    }
}
