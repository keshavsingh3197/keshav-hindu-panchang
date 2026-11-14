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
    public string ChannelName { get; set; } = string.Empty;
    public string ChannelUrl { get; set; } = string.Empty;
    public string SourceChannelHandle { get; set; } = string.Empty;
    public bool IsAutoSynced { get; set; }
    /// <summary>IST calendar date key (yyyy-MM-dd) used to filter daily &amp; historical videos by date.</summary>
    public string DateKey { get; set; } = string.Empty;
    /// <summary>Originating pipeline or feed type (e.g. HinduPanchang, HinduFestival, DailyKatha, AajKaItihaas, YouTubeSync).</summary>
    public string SourceType { get; set; } = string.Empty;
    /// <summary>Optional AnimStudio AutomationPost ID so repeated syncs upsert cleanly.</summary>
    public string AutomationPostId { get; set; } = string.Empty;
    /// <summary>Video format (Short or Video).</summary>
    public string Format { get; set; } = string.Empty;
}

public sealed record VideoFeedResponse(
    string? SelectedDate,
    bool ShowAllDates,
    string TodayDateKey,
    IReadOnlyList<string> AvailableDates,
    int TotalAllDatesCount,
    SpiritualVideo? FeaturedVideo,
    List<SpiritualVideo> Videos);

public sealed class AutomationVideoSyncRequest
{
    public string? AutomationPostId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? YouTubeUrl { get; set; }
    public string? YouTubeVideoId { get; set; }
    public string? Category { get; set; }
    public string? SourceType { get; set; }
    public string? DateKey { get; set; }
    public DateTime? PublishedDate { get; set; }
    public string? ChannelName { get; set; }
    public string? ChannelUrl { get; set; }
    public string? SourceChannelHandle { get; set; }
    public string? Format { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool IsFeaturedDailyVideo { get; set; }
}
