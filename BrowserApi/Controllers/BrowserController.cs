using BrowserAPI.Data;
using BrowserAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BrowserAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HistoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HistoryController(AppDbContext context)
        {
            _context = context;
        }

        public record CreateHistoryRequest(int UserId, string Url, string? Title, DateTime? VisitedAt);

        [HttpGet("{userId:int}")]
        public async Task<IActionResult> GetHistory(int userId)
        {
            var history = await _context.Histories
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.VisitedAt)
                .ToListAsync();
            return Ok(history);
        }

        [HttpPost]
        public async Task<IActionResult> SaveHistory([FromBody] CreateHistoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Url))
            {
                return BadRequest("Url is required");
            }

            var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId && u.IsActive);
            if (!userExists)
            {
                return BadRequest("User not found");
            }

            var historyItem = new BrowserHistory
            {
                UserId = request.UserId,
                Url = request.Url,
                Title = request.Title ?? string.Empty,
                VisitedAt = request.VisitedAt ?? DateTime.UtcNow
            };

            _context.Histories.Add(historyItem);
            await _context.SaveChangesAsync();

            return Ok(historyItem);
        }

        [HttpDelete("{userId:int}")]
        public async Task<IActionResult> ClearHistory(int userId)
        {
            var roleFromHeader = HttpContext.Request.Headers["X-Role"].ToString();
            var isAdmin = string.Equals(roleFromHeader, "Admin", StringComparison.OrdinalIgnoreCase);

            if (!isAdmin)
            {
                var canClear = await _context.UserPermissions
                    .AnyAsync(p => p.UserId == userId && p.CanClearHistory);
                if (!canClear)
                {
                    return Forbid();
                }
            }

            var historyRows = await _context.Histories.Where(h => h.UserId == userId).ToListAsync();
            _context.Histories.RemoveRange(historyRows);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
