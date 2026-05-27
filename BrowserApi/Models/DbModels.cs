using System.ComponentModel.DataAnnotations;

namespace BrowserAPI.Models
{
    public class User
    {
        [Key] public int Id { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
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
}
