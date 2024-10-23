using AutoMapper;
using Gauniv.WebServer.Data;
using Gauniv.WebServer.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Text;
using CommunityToolkit.HighPerformance.Memory;
using CommunityToolkit.HighPerformance;
using Microsoft.AspNetCore.Authorization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.AspNetCore.Identity;

namespace Gauniv.WebServer.Api
{
    [Route("api/1.0.0/[controller]/[action]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly ApplicationDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public GamesController(ApplicationDbContext appDbContext, IMapper mapper, UserManager<User> userManager)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _userManager = userManager;
        }
        [HttpGet]
        public ActionResult<IEnumerable<Game>> GetAllGames()
        {
            var games = _appDbContext.Games.ToList();
            return Ok(games);
        }

        [HttpPost]
        public ActionResult<Game> CreateGame([FromBody] Game game)
        {
            _appDbContext.Games.Add(game);
            _appDbContext.SaveChanges();
            return CreatedAtAction(nameof(GetGameById), new { id = game.Id }, game);
        }

        [HttpGet("{id}")]
        public ActionResult<Game> GetGameById(int id)
        {
            var game = _appDbContext.Games.Find(id);
            if (game == null) return NotFound();
            return Ok(game);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Game game)
        {
            if (ModelState.IsValid)
            {
                _appDbContext.Games.Add(game);
                _appDbContext.SaveChanges();
                return RedirectToAction("Index");  // Redirige vers une liste des jeux
            }
            return View(game);
        }

        private IActionResult View(Game game)
        {
            throw new NotImplementedException();
        }
    }
}