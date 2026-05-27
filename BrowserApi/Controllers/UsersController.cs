using BrowserAPI.Data;
using BrowserAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BrowserAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            if (!IsAdminRequest()) return Forbid();
            return Ok(await _context.Users.OrderBy(u => u.Id).ToListAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            if (!IsAdminRequest()) return Forbid();
            var user = await _context.Users.FindAsync(id);
            return user is null ? NotFound() : Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<User>> CreateUser([FromBody] User request)
        {
            if (!IsAdminRequest()) return Forbid();

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Username/password required");
            }

            var username = request.Username.Trim();
            var exists = await _context.Users.AnyAsync(u => u.Username == username);
            if (exists)
            {
                return Conflict("Username already exists");
            }

            var user = new User
            {
                DeviceId = request.DeviceId ?? string.Empty,
                Username = username,
                Password = request.Password,
                Role = string.IsNullOrWhiteSpace(request.Role) ? "User" : request.Role,
                IsActive = request.IsActive
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _context.UserPermissions.Add(new UserPermission
            {
                UserId = user.Id,
                CanClearHistory = true
            });
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<User>> UpdateUser(int id, [FromBody] User request)
        {
            if (!IsAdminRequest()) return Forbid();

            var user = await _context.Users.FindAsync(id);
            if (user is null) return NotFound();

            if (!string.IsNullOrWhiteSpace(request.Username) &&
                !string.Equals(request.Username, user.Username, StringComparison.OrdinalIgnoreCase))
            {
                var usernameExists = await _context.Users.AnyAsync(u => u.Id != id && u.Username == request.Username);
                if (usernameExists) return Conflict("Username already exists");
                user.Username = request.Username.Trim();
            }

            user.DeviceId = request.DeviceId ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(request.Password)) user.Password = request.Password;
            user.Role = string.IsNullOrWhiteSpace(request.Role) ? user.Role : request.Role;
            user.IsActive = request.IsActive;

            await _context.SaveChangesAsync();
            return Ok(user);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!IsAdminRequest()) return Forbid();

            var user = await _context.Users.FindAsync(id);
            if (user is null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool IsAdminRequest()
        {
            var roleFromHeader = HttpContext.Request.Headers["X-Role"].ToString();
            var roleFromQuery = HttpContext.Request.Query["role"].ToString();
            var role = string.IsNullOrWhiteSpace(roleFromHeader) ? roleFromQuery : roleFromHeader;
            return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
        }
    }
}
