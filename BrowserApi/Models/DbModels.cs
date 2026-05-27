using System.ComponentModel.DataAnnotations;

namespace BrowserAPI.Models
{
    public class User
    {
        [Key] public int Id { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public bool IsActive { get; set; } = true;
    }

    public class BrowserSetting
    {
        [Key] public int Id { get; set; }
        public int UserId { get; set; }
        public string DefaultSearchEngine { get; set; } = string.Empty;
        public string HomePage { get; set; } = string.Empty;
        public bool IsDarkMode { get; set; }
    }

    public class BrowserHistory
    {
        [Key] public int Id { get; set; }
        public int UserId { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime VisitedAt { get; set; }
    }

    public class SiteRule
    {
        [Key] public int Id { get; set; }
        public string UrlPattern { get; set; } = string.Empty;
        public bool? IsBlocked { get; set; }
        public string RuleType { get; set; } = "Allow";
        public string Scope { get; set; } = "Global";
        public int? UserId { get; set; }
        public bool IsEnabled { get; set; } = true;
        public int Priority { get; set; }
    }

    public class BrowserTab
    {
        [Key] public int Id { get; set; }
        public int UserId { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public bool IsPinned { get; set; }
    }

    public class UserPermission
    {
        [Key] public int Id { get; set; }
        public int UserId { get; set; }
        public bool CanClearHistory { get; set; }
    }
}
