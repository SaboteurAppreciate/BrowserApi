using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BrowserAPI.Data;
using BrowserAPI.Models;

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

        // ПОЛУЧИТЬ ИСТОРИЮ (Пока отдаем историю пользователя 1)
        [HttpGet]
        public async Task<IActionResult> GetHistory()
        {
            // Берем историю только для UserId = 1, сортируем от новых к старым
            var history = await _context.Histories
                                        .Where(h => h.UserId == 1)
                                        .OrderByDescending(h => h.VisitedAt)
                                        .ToListAsync();
            return Ok(history);
        }

        // СОХРАНИТЬ ИСТОРИЮ
        [HttpPost]
        public async Task<IActionResult> SaveHistory([FromBody] BrowserHistory historyItem)
        {
            // ВРЕМЕННЫЙ КОСТЫЛЬ: Пока нет авторизации в приложении,
            // принудительно записываем историю на Админа (UserId = 1)
            historyItem.UserId = 1;
            historyItem.VisitedAt = DateTime.UtcNow;

            _context.Histories.Add(historyItem);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}