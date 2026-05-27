using BrowserAPI.Data;
using BrowserAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BrowserAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        public record RegisterRequest(string Username, string Password, string? DeviceId);
        public record LoginRequest(string Username, string Password, string? DeviceId);
        public record AuthResponse(int UserId, string Username, string Role, string DeviceId);

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            var username = (request.Username ?? string.Empty).Trim();
            var password = request.Password ?? string.Empty;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return BadRequest("Username/password required");
            }

            var exists = await _context.Users.AnyAsync(u => u.Username == username);
            if (exists)
            {
                return Conflict("User already exists");
            }

            var user = new User
            {
                Username = username,
                Password = password,
                DeviceId = request.DeviceId?.Trim() ?? string.Empty,
                Role = "User",
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _context.UserPermissions.Add(new UserPermission
            {
                UserId = user.Id,
                CanClearHistory = true
            });
            await _context.SaveChangesAsync();

            return Ok(new AuthResponse(user.Id, user.Username, user.Role, user.DeviceId));
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            var username = (request.Username ?? string.Empty).Trim();
            var password = request.Password ?? string.Empty;
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Username == username &&
                u.Password == password &&
                u.IsActive);

            if (user is null)
            {
                return Unauthorized("Wrong username or password");
            }

            if (!string.IsNullOrWhiteSpace(request.DeviceId) && string.IsNullOrWhiteSpace(user.DeviceId))
            {
                user.DeviceId = request.DeviceId.Trim();
                await _context.SaveChangesAsync();
            }

            return Ok(new AuthResponse(user.Id, user.Username, user.Role, user.DeviceId));
        }
    }
}
