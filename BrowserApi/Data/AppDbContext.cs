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
        public DbSet<SiteRule> SiteRules { get; set; }
        public DbSet<BrowserTab> Tabs { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<BrowserSetting>()
                .HasIndex(s => s.UserId)
                .IsUnique();

            modelBuilder.Entity<UserPermission>()
                .HasIndex(p => p.UserId)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
