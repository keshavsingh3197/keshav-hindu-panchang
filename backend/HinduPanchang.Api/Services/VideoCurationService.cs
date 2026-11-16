using System.Globalization;
using System.Text.RegularExpressions;
using HinduPanchang.Api.Models;

namespace HinduPanchang.Api.Services;

public partial class VideoCurationService
{
    private const string Collection = "spiritual_videos";
    private readonly HybridDataStore _store;

    public VideoCurationService(HybridDataStore store)
    {
        _store = store;
    }

    public async Task<List<SpiritualVideo>> GetVideosAsync(
        string? category = null,
        string? date = null,
        bool allDates = true,
        string? channel = null,
        bool includePersonal = false)
    {
        var list = await _store.GetListAsync(Collection, PanchangSeedData.DefaultVideos);
        var filtered = FilterPanchangVideos(list, category, channel, includePersonal);

        if (allDates)
            return filtered;

        var normalizedDate = NormalizeDateQuery(date);
        if (!string.IsNullOrWhiteSpace(normalizedDate))
            return filtered.Where(v => v.DateKey == normalizedDate).ToList();

        var todayKey = GetIstTodayDateKey();
        var todayVideos = filtered.Where(v => v.DateKey == todayKey).ToList();
        if (todayVideos.Count > 0)
            return todayVideos;

        var latestDateKey = filtered
            .Select(v => v.DateKey)
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .OrderByDescending(d => d, StringComparer.Ordinal)
            .FirstOrDefault();

        return string.IsNullOrWhiteSpace(latestDateKey)
            ? []
            : filtered.Where(v => v.DateKey == latestDateKey).ToList();
    }

    public async Task<VideoFeedResponse> GetVideoFeedAsync(
        string? category = null,
        string? date = null,
        bool allDates = false,
        string? channel = null,
        bool includePersonal = false)
    {
        var list = await _store.GetListAsync(Collection, PanchangSeedData.DefaultVideos);
        var filtered = FilterPanchangVideos(list, category, channel, includePersonal);

        var todayKey = GetIstTodayDateKey();
        var availableDates = filtered
            .Select(v => v.DateKey)
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Distinct(StringComparer.Ordinal)
            .OrderByDescending(d => d, StringComparer.Ordinal)
            .ToList();

        var normalizedDate = NormalizeDateQuery(date);
        string? selectedDate;
        if (allDates)
        {
            selectedDate = normalizedDate;
        }
        else if (!string.IsNullOrWhiteSpace(normalizedDate))
        {
            selectedDate = normalizedDate;
        }
        else if (availableDates.Contains(todayKey, StringComparer.Ordinal))
        {
            selectedDate = todayKey;
        }
        else
        {
            selectedDate = availableDates.FirstOrDefault() ?? todayKey;
        }

        var videos = allDates
            ? filtered
            : filtered.Where(v => v.DateKey == selectedDate).ToList();

        var featured = videos.FirstOrDefault(v => v.IsFeaturedDailyVideo)
            ?? videos.FirstOrDefault()
            ?? filtered.FirstOrDefault(v => v.IsFeaturedDailyVideo)
            ?? filtered.FirstOrDefault();

        return new VideoFeedResponse(
            selectedDate,
            allDates,
            todayKey,
            availableDates,
            filtered.Count,
            featured,
            videos);
    }

    public async Task<SpiritualVideo?> GetFeaturedVideoAsync()
    {
        var videos = await GetVideosAsync(allDates: true);
        return videos.FirstOrDefault(v => v.IsFeaturedDailyVideo) ?? videos.FirstOrDefault();
    }

    public async Task<int> UpsertSyncedVideosAsync(IEnumerable<SpiritualVideo> syncedVideos)
    {
        var incoming = syncedVideos
            .Where(v => !string.IsNullOrWhiteSpace(v.YouTubeVideoId)
                && !IsLegacyDemoVideo(v)
                && !IsExcludedPersonalChannel(v))
            .ToList();
        if (incoming.Count == 0) return 0;

        var existing = (await _store.GetListAsync(Collection, PanchangSeedData.DefaultVideos))
            .Where(v => !IsLegacyDemoVideo(v))
            .ToList();

        bool hasFeatured = existing.Any(v => v.IsFeaturedDailyVideo);
        var byVideoId = existing
            .Where(v => !string.IsNullOrWhiteSpace(v.YouTubeVideoId))
            .GroupBy(v => v.YouTubeVideoId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        int count = 0;
        var upsertList = new List<SpiritualVideo>();
        foreach (var v in incoming)
        {
            v.DateKey = ResolveDateKey(v);
            if (string.IsNullOrWhiteSpace(v.SourceType))
                v.SourceType = "YouTubeSync";

            if (byVideoId.TryGetValue(v.YouTubeVideoId, out var cur))
            {
                v.Id = cur.Id;
                v.IsFeaturedDailyVideo = cur.IsFeaturedDailyVideo;
                if (v.ViewCount == 0 && cur.ViewCount > 0) v.ViewCount = cur.ViewCount;
                if (string.IsNullOrWhiteSpace(v.AutomationPostId) && !string.IsNullOrWhiteSpace(cur.AutomationPostId))
                    v.AutomationPostId = cur.AutomationPostId;
                if (string.IsNullOrWhiteSpace(v.Format) && !string.IsNullOrWhiteSpace(cur.Format))
                    v.Format = cur.Format;
                if (!string.IsNullOrWhiteSpace(cur.SourceType) && v.SourceType == "YouTubeSync")
                    v.SourceType = cur.SourceType;
                existing.Remove(cur);
            }
            existing.Add(v);
            byVideoId[v.YouTubeVideoId] = v;
            upsertList.Add(v);
            count++;
        }

        if (!hasFeatured && upsertList.Count > 0)
        {
            var latest = upsertList.OrderByDescending(v => v.PublishedDate).First();
            latest.IsFeaturedDailyVideo = true;
        }

        foreach (var v in upsertList)
            await _store.UpsertItemAsync(Collection, v.Id, x => x.Id, v, PanchangSeedData.DefaultVideos);

        return count;
    }

    public async Task<int> PurgeLegacyDemoVideosAsync()
    {
        var all = await _store.GetListAsync(Collection, PanchangSeedData.DefaultVideos);
        var legacyItems = all.Where(IsLegacyDemoVideo).ToList();
        int removed = 0;
        foreach (var legacy in legacyItems)
        {
            if (await _store.DeleteItemAsync(Collection, legacy.Id, x => x.Id, PanchangSeedData.DefaultVideos))
                removed++;
        }
        return removed;
    }

    public static bool IsLegacyDemoVideo(SpiritualVideo v)
    {
        if (string.Equals(v.YouTubeVideoId, "jNQXAC9IVRw", StringComparison.OrdinalIgnoreCase))
            return true;
        if (!string.IsNullOrWhiteSpace(v.ChannelUrl) && v.ChannelUrl.Contains("@KeshavSingh", StringComparison.OrdinalIgnoreCase) && !v.ChannelUrl.Contains("keshavsinghworld", StringComparison.OrdinalIgnoreCase))
            return true;
        if (string.Equals(v.ChannelName, "Keshav Singh Official", StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }

    public static bool IsExcludedPersonalChannel(SpiritualVideo v)
    {
        return ContainsPersonalHandle(v.SourceChannelHandle)
            || ContainsPersonalHandle(v.ChannelUrl)
            || ContainsPersonalHandle(v.ChannelName);
    }

    /// <summary>
    /// A video someone chose to publish to Panchang (from AnimStudio Auto-Post or the admin console).
    /// It is shown whatever channel it is on; only the automatic YouTube feed sync skips personal channels.
    /// </summary>
    public static bool IsExplicitlyPublished(SpiritualVideo v) =>
        !string.IsNullOrWhiteSpace(v.AutomationPostId);

    internal static bool IsExcludedPersonalChannelHandleOrName(string? handle, string? url, string? name) =>
        ContainsPersonalHandle(handle) || ContainsPersonalHandle(url) || ContainsPersonalHandle(name);

    private static bool ContainsPersonalHandle(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        return value.Contains("keshavsinghworld", StringComparison.OrdinalIgnoreCase)
            || value.Contains("Keshav Singh World", StringComparison.OrdinalIgnoreCase);
    }

    public static SpiritualVideo MapAutomationSyncRequest(AutomationVideoSyncRequest req)
    {
        var ytId = !string.IsNullOrWhiteSpace(req.YouTubeVideoId)
            ? req.YouTubeVideoId.Trim()
            : (!string.IsNullOrWhiteSpace(req.YouTubeUrl) ? ExtractYouTubeId(req.YouTubeUrl) : string.Empty);

        var postId = req.AutomationPostId?.Trim() ?? string.Empty;
        var id = !string.IsNullOrWhiteSpace(postId)
            ? $"auto-{postId}"
            : (!string.IsNullOrWhiteSpace(ytId) ? $"yt-{ytId}" : $"vid-{Guid.NewGuid():N}"[..10]);

        var published = req.PublishedDate is { } dt && dt != default
            ? (dt.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(dt, DateTimeKind.Utc) : dt.ToUniversalTime())
            : DateTime.UtcNow;

        var format = req.Format?.Trim() ?? string.Empty;
        var isShort = string.Equals(format, "Short", StringComparison.OrdinalIgnoreCase)
            || string.Equals(format, "Vertical", StringComparison.OrdinalIgnoreCase);

        var video = new SpiritualVideo
        {
            Id = id,
            Title = req.Title?.Trim() ?? "Untitled Video",
            Description = req.Description?.Trim() ?? string.Empty,
            YouTubeUrl = !string.IsNullOrWhiteSpace(req.YouTubeUrl)
                ? req.YouTubeUrl.Trim()
                : (!string.IsNullOrWhiteSpace(ytId) ? $"https://www.youtube.com/watch?v={ytId}" : string.Empty),
            YouTubeVideoId = ytId,
            EmbedUrl = !string.IsNullOrWhiteSpace(ytId) ? $"https://www.youtube.com/embed/{ytId}" : string.Empty,
            ThumbnailUrl = !string.IsNullOrWhiteSpace(req.ThumbnailUrl)
                ? req.ThumbnailUrl.Trim()
                : (!string.IsNullOrWhiteSpace(ytId) ? $"https://i.ytimg.com/vi/{ytId}/hqdefault.jpg" : string.Empty),
            Category = !string.IsNullOrWhiteSpace(req.Category) ? req.Category.Trim() : InferCategoryFromSourceType(req.SourceType),
            PublishedDate = published,
            DurationMinutes = isShort ? "Short" : "Video",
            IsFeaturedDailyVideo = req.IsFeaturedDailyVideo,
            ChannelName = req.ChannelName?.Trim() ?? string.Empty,
            ChannelUrl = req.ChannelUrl?.Trim() ?? string.Empty,
            SourceChannelHandle = req.SourceChannelHandle?.Trim() ?? string.Empty,
            IsAutoSynced = true,
            DateKey = req.DateKey?.Trim() ?? string.Empty,
            SourceType = !string.IsNullOrWhiteSpace(req.SourceType) ? req.SourceType.Trim() : "Automation",
            AutomationPostId = postId,
            Format = format
        };
        video.DateKey = ResolveDateKey(video);
        return video;
    }

    public async Task<SpiritualVideo> AddOrUpdateVideoAsync(SpiritualVideo video)
    {
        if (string.IsNullOrWhiteSpace(video.YouTubeVideoId) && !string.IsNullOrWhiteSpace(video.YouTubeUrl))
            video.YouTubeVideoId = ExtractYouTubeId(video.YouTubeUrl);

        var all = await _store.GetListAsync(Collection, PanchangSeedData.DefaultVideos);

        var existingMatch = (!string.IsNullOrWhiteSpace(video.AutomationPostId)
                ? all.FirstOrDefault(x => string.Equals(x.AutomationPostId, video.AutomationPostId, StringComparison.OrdinalIgnoreCase))
                : null)
            ?? (!string.IsNullOrWhiteSpace(video.YouTubeVideoId)
                ? all.FirstOrDefault(x => string.Equals(x.YouTubeVideoId, video.YouTubeVideoId, StringComparison.OrdinalIgnoreCase))
                : null);

        if (existingMatch != null)
        {
            video.Id = existingMatch.Id;
            if (video.ViewCount == 0 && existingMatch.ViewCount > 0)
                video.ViewCount = existingMatch.ViewCount;
        }
        else if (string.IsNullOrWhiteSpace(video.Id))
        {
            video.Id = $"vid-{Guid.NewGuid():N}"[..10];
        }

        if (!string.IsNullOrWhiteSpace(video.YouTubeVideoId))
        {
            if (string.IsNullOrWhiteSpace(video.YouTubeUrl))
                video.YouTubeUrl = $"https://www.youtube.com/watch?v={video.YouTubeVideoId}";
            if (string.IsNullOrWhiteSpace(video.EmbedUrl))
                video.EmbedUrl = $"https://www.youtube.com/embed/{video.YouTubeVideoId}";
            if (string.IsNullOrWhiteSpace(video.ThumbnailUrl))
                video.ThumbnailUrl = $"https://img.youtube.com/vi/{video.YouTubeVideoId}/hqdefault.jpg";
        }

        if (video.PublishedDate == default)
            video.PublishedDate = DateTime.UtcNow;

        video.DateKey = ResolveDateKey(video);

        if (video.IsFeaturedDailyVideo)
        {
            foreach (var other in all.Where(v => v.IsFeaturedDailyVideo && v.Id != video.Id))
            {
                other.IsFeaturedDailyVideo = false;
                await _store.UpsertItemAsync(Collection, other.Id, x => x.Id, other, PanchangSeedData.DefaultVideos);
            }
        }

        return await _store.UpsertItemAsync(Collection, video.Id, x => x.Id, video, PanchangSeedData.DefaultVideos);
    }

    private static List<SpiritualVideo> FilterPanchangVideos(
        IEnumerable<SpiritualVideo> list,
        string? category,
        string? channel,
        bool includePersonal)
    {
        var query = list
            .Where(v => !IsLegacyDemoVideo(v))
            .Where(v => includePersonal || !IsExcludedPersonalChannel(v) || IsExplicitlyPublished(v))
            .Select(v =>
            {
                if (string.IsNullOrWhiteSpace(v.DateKey))
                    v.DateKey = ResolveDateKey(v);
                return v;
            });

        if (!string.IsNullOrWhiteSpace(category) && !category.Equals("All", StringComparison.OrdinalIgnoreCase))
            query = query.Where(v => v.Category.Equals(category.Trim(), StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(channel) && !channel.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            var needle = channel.Trim().TrimStart('@');
            query = query.Where(v =>
                (!string.IsNullOrWhiteSpace(v.SourceChannelHandle) && v.SourceChannelHandle.TrimStart('@').Equals(needle, StringComparison.OrdinalIgnoreCase))
                || (!string.IsNullOrWhiteSpace(v.ChannelName) && v.ChannelName.Contains(channel.Trim(), StringComparison.OrdinalIgnoreCase))
                || (!string.IsNullOrWhiteSpace(v.ChannelUrl) && v.ChannelUrl.Contains(needle, StringComparison.OrdinalIgnoreCase)));
        }

        return query
            .OrderByDescending(v => v.IsFeaturedDailyVideo)
            .ThenByDescending(v => v.DateKey, StringComparer.Ordinal)
            .ThenByDescending(v => v.PublishedDate)
            .ToList();
    }

    public static string GetIstTodayDateKey() => ToIstDateKey(DateTime.UtcNow);

    public static string ResolveDateKey(SpiritualVideo v)
    {
        if (!string.IsNullOrWhiteSpace(v.DateKey) &&
            DateTime.TryParseExact(v.DateKey.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return parsed.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        var dt = v.PublishedDate == default ? DateTime.UtcNow : v.PublishedDate;
        return ToIstDateKey(dt);
    }

    internal static string ToIstDateKey(DateTime dt)
    {
        var utc = dt.Kind switch
        {
            DateTimeKind.Utc => dt,
            DateTimeKind.Local => dt.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
        };
        var ist = utc.AddHours(5).AddMinutes(30);
        return ist.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    private static string? NormalizeDateQuery(string? date)
    {
        if (string.IsNullOrWhiteSpace(date)) return null;
        var trimmed = date.Trim();
        if (DateTime.TryParseExact(trimmed, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var exact))
            return exact.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        if (DateTime.TryParse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            return parsed.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return trimmed;
    }

    private static string InferCategoryFromSourceType(string? sourceType)
    {
        if (string.IsNullOrWhiteSpace(sourceType)) return "Daily News & Rashifal";
        if (sourceType.Contains("Katha", StringComparison.OrdinalIgnoreCase)
            || sourceType.Contains("Festival", StringComparison.OrdinalIgnoreCase))
            return "Pauranik Katha";
        if (sourceType.Contains("Mantra", StringComparison.OrdinalIgnoreCase))
            return "Mantra & Meditation";
        if (sourceType.Contains("Temple", StringComparison.OrdinalIgnoreCase))
            return "Temple Darshan";
        return "Daily News & Rashifal";
    }

    /// <summary>Takes a video published from AnimStudio Auto-Post off the hub; false when none matched.</summary>
    public async Task<bool> RemoveByAutomationPostIdAsync(string automationPostId)
    {
        if (string.IsNullOrWhiteSpace(automationPostId)) return false;
        var postId = automationPostId.Trim();
        var all = await _store.GetListAsync(Collection, PanchangSeedData.DefaultVideos);
        var removed = false;
        foreach (var v in all.Where(x => string.Equals(x.AutomationPostId, postId, StringComparison.OrdinalIgnoreCase)).ToList())
            removed |= await _store.DeleteItemAsync(Collection, v.Id, x => x.Id, PanchangSeedData.DefaultVideos);
        return removed;
    }

    public Task<bool> DeleteVideoAsync(string id) =>
        _store.DeleteItemAsync(Collection, id, x => x.Id, PanchangSeedData.DefaultVideos);

    private static string ExtractYouTubeId(string url)
    {
        var match = YouTubeIdRegex().Match(url);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    [GeneratedRegex(@"(?:v=|youtu\.be/|embed/|shorts/)([A-Za-z0-9_-]{6,15})")]
    private static partial Regex YouTubeIdRegex();
}
