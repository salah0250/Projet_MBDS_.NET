using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gauniv.WebServer.Data;
using Gauniv.WebServer.Dtos;
using AutoMapper;

namespace Gauniv.WebServer.Api
{
    [Route("api/1.0.0/[controller]/[action]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public UsersController(
            ApplicationDbContext context,
            UserManager<User> userManager,
            IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }

        // GET: api/1.0.0/Users/GetUserProfile
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<UserDto>> GetUserProfile()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _context.Users
                .Include(u => u.UserGames)
                    .ThenInclude(ug => ug.Game)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound();

            var userDto = _mapper.Map<UserDto>(user);
            return Ok(userDto);
        }

        // GET: api/1.0.0/Users/GetUserLibrary
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetUserLibrary()
        {
            var userId = _userManager.GetUserId(User);
            var userGames = await _context.UserGames
                .Include(ug => ug.Game)
                .Where(ug => ug.UserId == userId)
                .Select(ug => ug.Game)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<GameDto>>(userGames));
        }

        // GET: api/1.0.0/Users/GetRecentPurchases
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserGameDto>>> GetRecentPurchases(int count = 10)
        {
            var userId = _userManager.GetUserId(User);
            var recentPurchases = await _context.UserGames
                .Include(ug => ug.Game)
                .Where(ug => ug.UserId == userId)
                .OrderByDescending(ug => ug.PurchaseDate)
                .Take(count)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<UserGameDto>>(recentPurchases));
        }

        // PUT: api/1.0.0/Users/UpdateProfile
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto profileDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            // Update user properties
            user.Email = profileDto.Email;
            user.FirstName = profileDto.FirstName;
            user.LastName = profileDto.LastName;
            user.PhoneNumber = profileDto.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(_mapper.Map<UserDto>(user));
        }

        // GET: api/1.0.0/Users/GetAllUsers
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var users = await _context.Users
                .Include(u => u.UserGames)
                    .ThenInclude(ug => ug.Game)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<UserDto>>(users));
        }

        // POST: api/1.0.0/Users/AssignRole
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AssignRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok();
        }

        // POST: api/1.0.0/Users/RemoveRole
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> RemoveRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok();
        }

        // DELETE: api/1.0.0/Users/DeleteUser
        [Authorize(Roles = "Admin")]
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok();
        }
    }
}