using System.Text.RegularExpressions;
using BrowserAPI.Data;
using BrowserAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BrowserAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SiteRulesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SiteRulesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SiteRule>>> GetRules([FromQuery] int? userId = null)
        {
            var query = _context.SiteRules.AsQueryable();
            if (userId.HasValue)
            {
                query = query.Where(r => r.Scope == "Global" || (r.Scope == "User" && r.UserId == userId));
            }

            var rules = await query.OrderByDescending(r => r.Priority).ThenBy(r => r.Id).ToListAsync();
            return Ok(rules);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<SiteRule>> GetRule(int id)
        {
            var rule = await _context.SiteRules.FindAsync(id);
            return rule is null ? NotFound() : Ok(rule);
        }

        [HttpPost]
        public async Task<ActionResult<SiteRule>> CreateRule([FromBody] SiteRule rule)
        {
            NormalizeRule(rule);
            _context.SiteRules.Add(rule);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetRule), new { id = rule.Id }, rule);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<SiteRule>> UpdateRule(int id, [FromBody] SiteRule request)
        {
            var rule = await _context.SiteRules.FindAsync(id);
            if (rule is null) return NotFound();

            rule.UrlPattern = request.UrlPattern;
            rule.RuleType = request.RuleType;
            rule.IsBlocked = request.IsBlocked;
            rule.Scope = request.Scope;
            rule.UserId = request.UserId;
            rule.IsEnabled = request.IsEnabled;
            rule.Priority = request.Priority;
            NormalizeRule(rule);

            await _context.SaveChangesAsync();
            return Ok(rule);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteRule(int id)
        {
            var rule = await _context.SiteRules.FindAsync(id);
            if (rule is null) return NotFound();

            _context.SiteRules.Remove(rule);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("check")]
        public async Task<IActionResult> Check([FromQuery] string url, [FromQuery] int? userId = null)
        {
            if (string.IsNullOrWhiteSpace(url)) return BadRequest("url is required");

            var rules = await _context.SiteRules
                .Where(r => r.IsEnabled)
                .Where(r => r.Scope == "Global" || (r.Scope == "User" && r.UserId == userId))
                .OrderByDescending(r => r.Priority)
                .ThenByDescending(r => r.Id)
                .ToListAsync();

            foreach (var rule in rules)
            {
                if (!IsMatch(url, rule.UrlPattern)) continue;

                var isBlocked = ResolveBlocked(rule);
                return Ok(new
                {
                    Allowed = !isBlocked,
                    MatchedRuleId = rule.Id,
                    RuleType = isBlocked ? "Block" : "Allow"
                });
            }

            return Ok(new
            {
                Allowed = true,
                MatchedRuleId = (int?)null,
                RuleType = "DefaultAllow"
            });
        }

        private static bool IsMatch(string url, string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern)) return false;
            var regexPattern = "^" + Regex.Escape(pattern.Trim()).Replace("\\*", ".*") + "$";
            return Regex.IsMatch(
                url,
                regexPattern,
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
                TimeSpan.FromMilliseconds(200));
        }

        private static bool ResolveBlocked(SiteRule rule)
        {
            if (!string.IsNullOrWhiteSpace(rule.RuleType))
            {
                return string.Equals(rule.RuleType, "Block", StringComparison.OrdinalIgnoreCase);
            }

            return rule.IsBlocked ?? false;
        }

        private static void NormalizeRule(SiteRule rule)
        {
            rule.UrlPattern = (rule.UrlPattern ?? string.Empty).Trim();
            rule.Scope = string.Equals(rule.Scope, "User", StringComparison.OrdinalIgnoreCase) ? "User" : "Global";

            if (string.IsNullOrWhiteSpace(rule.RuleType))
            {
                rule.RuleType = (rule.IsBlocked ?? false) ? "Block" : "Allow";
            }
            else
            {
                rule.RuleType = string.Equals(rule.RuleType, "Block", StringComparison.OrdinalIgnoreCase)
                    ? "Block"
                    : "Allow";
            }
        }
    }
}
