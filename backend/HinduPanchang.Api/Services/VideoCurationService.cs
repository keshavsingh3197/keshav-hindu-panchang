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
        var ordered = list.OrderByDescending(v => v.IsFeaturedDailyVideo).ThenByDescending(v => v.PublishedDate);
        if (string.IsNullOrWhiteSpace(category) || category.Equals("All", StringComparison.OrdinalIgnoreCase))
            return ordered.ToList();

        return ordered.Where(v => v.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<SpiritualVideo?> GetFeaturedVideoAsync()
    {
        var videos = await GetVideosAsync();
        return videos.FirstOrDefault(v => v.IsFeaturedDailyVideo) ?? videos.FirstOrDefault();
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
