using BrowserAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BrowserAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<BrowserSetting> Settings { get; set; }
        public DbSet<BrowserHistory> Histories { get; set; }
    }
}
