namespace HinduPanchang.Api.Models;

public class SpiritualVideo
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string YouTubeUrl { get; set; } = string.Empty;
    public string YouTubeVideoId { get; set; } = string.Empty;
    public string EmbedUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string Category { get; set; } = "Daily News & Rashifal"; // "Daily News & Rashifal", "Pauranik Katha", "Mantra & Meditation", "Temple Darshan"
    public DateTime PublishedDate { get; set; } = DateTime.UtcNow;
    public string DurationMinutes { get; set; } = "12:45";
    public long ViewCount { get; set; }
    public bool IsFeaturedDailyVideo { get; set; }
    public string ChannelName { get; set; } = "Keshav Singh Official";
    public string ChannelUrl { get; set; } = "https://youtube.com/@KeshavSingh";
}
