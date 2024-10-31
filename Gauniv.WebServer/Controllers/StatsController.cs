using Gauniv.WebServer.Data;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Gauniv.WebServer.Controllers
{
    public class StatsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StatsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var stats = new
            {
                TotalGames = _context.Games.Count(),
                GamesByCategory = _context.Games
                    .GroupBy(g => g.Category)
                    .Select(g => new { Category = g.Key, Count = g.Count() })
                    .ToDictionary(x => x.Category, x => x.Count),
                TopGame = _context.Games
                    .OrderByDescending(g => g.UserGames.Count)
                    .Select(g => new { g.Title, PlayerCount = g.UserGames.Count })
                    .FirstOrDefault(),
                TopPlayer = _context.Users
                    .OrderByDescending(u => u.UserGames.Count)
                    .Select(u => new { u.UserName, GameCount = u.UserGames.Count })
                    .FirstOrDefault(),
                PriceDistribution = GetPriceDistribution() // Ajoutez cette ligne
            };

            // Transforme Keys et Values en List pour éviter l'erreur de binding
            var viewModel = new
            {
                stats.TotalGames,
                GamesByCategoryKeys = stats.GamesByCategory.Keys.ToList(),
                GamesByCategoryValues = stats.GamesByCategory.Values.ToList(),
                CategoryNames = stats.GamesByCategory.Keys.Select(c => c.ToString()).ToList(),
                stats.TopGame,
                stats.TopPlayer,
                stats.PriceDistribution // Ajoutez la distribution des prix ici
            };

            return View(viewModel);
        }

        // Méthode pour calculer la distribution des prix
        private List<int> GetPriceDistribution()
        {
            var priceRanges = new int[6]; // 0-10, 11-20, 21-30, 31-40, 41-50, 50+

            foreach (var game in _context.Games)
            {
                if (game.Price <= 10) priceRanges[0]++;
                else if (game.Price <= 20) priceRanges[1]++;
                else if (game.Price <= 30) priceRanges[2]++;
                else if (game.Price <= 40) priceRanges[3]++;
                else if (game.Price <= 50) priceRanges[4]++;
                else priceRanges[5]++;
            }

            return priceRanges.ToList();
        }

    }
}
