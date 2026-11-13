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

    public async Task<List<SpiritualVideo>> GetVideosAsync(string? category = null)
    {
        var list = await _store.GetListAsync(Collection, PanchangSeedData.DefaultVideos);
        var clean = list.Where(v => !IsLegacyDemoVideo(v));
        var ordered = clean.OrderByDescending(v => v.IsFeaturedDailyVideo).ThenByDescending(v => v.PublishedDate);
        if (string.IsNullOrWhiteSpace(category) || category.Equals("All", StringComparison.OrdinalIgnoreCase))
            return ordered.ToList();

        return ordered.Where(v => v.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<SpiritualVideo?> GetFeaturedVideoAsync()
    {
        var videos = await GetVideosAsync();
        return videos.FirstOrDefault(v => v.IsFeaturedDailyVideo) ?? videos.FirstOrDefault();
    }

    public async Task<int> UpsertSyncedVideosAsync(IEnumerable<SpiritualVideo> syncedVideos)
    {
        var incoming = syncedVideos
            .Where(v => !string.IsNullOrWhiteSpace(v.YouTubeVideoId) && !IsLegacyDemoVideo(v))
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
            if (byVideoId.TryGetValue(v.YouTubeVideoId, out var cur))
            {
                v.Id = cur.Id;
                v.IsFeaturedDailyVideo = cur.IsFeaturedDailyVideo;
                if (v.ViewCount == 0 && cur.ViewCount > 0) v.ViewCount = cur.ViewCount;
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
        if (!string.IsNullOrWhiteSpace(v.ChannelUrl) && v.ChannelUrl.Contains("@KeshavSingh", StringComparison.OrdinalIgnoreCase))
            return true;
        if (string.Equals(v.ChannelName, "Keshav Singh Official", StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }

    public async Task<SpiritualVideo> AddOrUpdateVideoAsync(SpiritualVideo video)
    {
        if (string.IsNullOrWhiteSpace(video.Id))
            video.Id = $"vid-{Guid.NewGuid():N}"[..10];

        if (string.IsNullOrWhiteSpace(video.YouTubeVideoId) && !string.IsNullOrWhiteSpace(video.YouTubeUrl))
            video.YouTubeVideoId = ExtractYouTubeId(video.YouTubeUrl);

        if (!string.IsNullOrWhiteSpace(video.YouTubeVideoId))
        {
            if (string.IsNullOrWhiteSpace(video.EmbedUrl))
                video.EmbedUrl = $"https://www.youtube.com/embed/{video.YouTubeVideoId}";
            if (string.IsNullOrWhiteSpace(video.ThumbnailUrl))
                video.ThumbnailUrl = $"https://img.youtube.com/vi/{video.YouTubeVideoId}/hqdefault.jpg";
        }

        if (video.PublishedDate == default)
            video.PublishedDate = DateTime.UtcNow;

        if (video.IsFeaturedDailyVideo)
        {
            var all = await _store.GetListAsync(Collection, PanchangSeedData.DefaultVideos);
            foreach (var other in all.Where(v => v.IsFeaturedDailyVideo && v.Id != video.Id))
            {
                other.IsFeaturedDailyVideo = false;
                await _store.UpsertItemAsync(Collection, other.Id, x => x.Id, other, PanchangSeedData.DefaultVideos);
            }
        }

        return await _store.UpsertItemAsync(Collection, video.Id, x => x.Id, video, PanchangSeedData.DefaultVideos);
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
