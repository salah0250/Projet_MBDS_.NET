using AutoMapper;
using Gauniv.WebServer.Data;
using Gauniv.WebServer.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Gauniv.WebServer.Api
{
    [Route("api/1.0.0/[controller]/[action]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly ApplicationDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public GamesController(
            ApplicationDbContext appDbContext,
            IMapper mapper,
            UserManager<User> userManager)
        {
            _appDbContext = appDbContext ?? throw new ArgumentNullException(nameof(appDbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [HttpGet]
        public ActionResult<IEnumerable<GameDto>> GetAllGames()
        {
            try
            {
                var games = _appDbContext.Games
                    .Include(g => g.UserGames)
                        .ThenInclude(ug => ug.User)
                    .ToList();

                var gameDtos = _mapper.Map<IEnumerable<GameDto>>(games);
                return Ok(gameDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public ActionResult<GameDto> CreateGame([FromBody] GameDto gameDto)
        {
            try
            {
                if (gameDto == null)
                    return BadRequest("Game data is null");

                var game = _mapper.Map<Game>(gameDto);
                _appDbContext.Games.Add(game);
                _appDbContext.SaveChanges();

                var createdGameDto = _mapper.Map<GameDto>(game);
                return CreatedAtAction(nameof(GetGameById), new { id = game.Id }, createdGameDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public ActionResult<GameDto> GetGameById(int id)
        {
            try
            {
                var game = _appDbContext.Games
                    .Include(g => g.UserGames)
                        .ThenInclude(ug => ug.User)
                    .FirstOrDefault(g => g.Id == id);

                if (game == null)
                    return NotFound($"Game with ID {id} not found");

                var gameDto = _mapper.Map<GameDto>(game);
                return Ok(gameDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public ActionResult<GameDto> UpdateGame(int id, [FromBody] GameUpdateDto gameUpdateDto)
        {
            try
            {
                if (gameUpdateDto == null)
                    return BadRequest("Game update data is null");

                var game = _appDbContext.Games.Find(id);
                if (game == null)
                    return NotFound($"Game with ID {id} not found");

                _mapper.Map(gameUpdateDto, game);
                _appDbContext.SaveChanges();

                var updatedGameDto = _mapper.Map<GameDto>(game);
                return Ok(updatedGameDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteGame(int id)
        {
            try
            {
                var game = _appDbContext.Games.Find(id);
                if (game == null)
                    return NotFound($"Game with ID {id} not found");

                _appDbContext.Games.Remove(game);
                _appDbContext.SaveChanges();
                return NoContent(); // Success, but no content to return
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPost("{id}/purchase")]
        public async Task<ActionResult> PurchaseGame(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (_appDbContext.UserGames.Any(ug => ug.UserId == userId && ug.GameId == id))
            {
                return BadRequest("User already owns this game");
            }

            var userGame = new UserGame
            {
                UserId = userId,
                GameId = id
            };

            _appDbContext.UserGames.Add(userGame);
            await _appDbContext.SaveChangesAsync();

            return Ok("Game purchased successfully");
        }
        [HttpGet("filter")]
        public ActionResult<IEnumerable<GameDto>> GetFilteredGames(
            string searchTerm = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            Category? category = null)
        {
            try
            {
                var gamesQuery = _appDbContext.Games.Include(g => g.UserGames)
                    .AsQueryable();

                // Filtrage par nom (Title)
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    searchTerm = searchTerm.ToLower().Trim();
                    gamesQuery = gamesQuery.Where(g => g.Title.ToLower().Contains(searchTerm) ||
                                                        (g.Description != null && g.Description.ToLower().Contains(searchTerm)));
                }

                // Filtrage par prix minimum
                if (minPrice.HasValue)
                {
                    gamesQuery = gamesQuery.Where(g => g.Price >= minPrice.Value);
                }

                // Filtrage par prix maximum
                if (maxPrice.HasValue)
                {
                    gamesQuery = gamesQuery.Where(g => g.Price <= maxPrice.Value);
                }

                // Filtrage par catégorie
                if (category.HasValue)
                {
                    gamesQuery = gamesQuery.Where(g => g.Category == category.Value);
                }

                var games = gamesQuery.ToList();
                var gameDtos = _mapper.Map<IEnumerable<GameDto>>(games);
                return Ok(gameDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Download(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                // Vérifier si l'utilisateur a acheté le jeu
                var userOwnsGame = await _appDbContext.UserGames
                    .AnyAsync(ug => ug.UserId == userId && ug.GameId == id);

                if (!userOwnsGame)
                {
                    return Forbid("User has not purchased this game");
                }

                // Récupérer le jeu avec son payload
                var game = await _appDbContext.Games
                    .Where(g => g.Id == id)
                    .Select(g => new { g.Title, g.Payload })
                    .FirstOrDefaultAsync();

                if (game == null)
                {
                    return NotFound($"Game with ID {id} not found");
                }

                if (game.Payload == null || game.Payload.Length == 0)
                {
                    return NotFound("Game payload is not available");
                }

                // Définir les headers appropriés
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{game.Title}.zip\"");
                Response.Headers.Add("Content-Type", "application/zip");
                Response.Headers.Add("Content-Length", game.Payload.Length.ToString());

                // Retourner le fichier avec le type MIME correct
                return File(game.Payload, "application/zip", $"{game.Title}.zip");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Optionnel : Endpoint pour vérifier si un utilisateur peut télécharger un jeu
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<bool>> CanDownload(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return false;
                }

                var userOwnsGame = await _appDbContext.UserGames
                    .AnyAsync(ug => ug.UserId == userId && ug.GameId == id);

                return Ok(userOwnsGame);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
