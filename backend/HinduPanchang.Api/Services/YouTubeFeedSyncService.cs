using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using HinduPanchang.Api.Models;

namespace HinduPanchang.Api.Services;

/// <summary>
/// Resolves YouTube @handle channels and fetches live spiritual/cultural videos via
/// YouTube Data API v3 (when configured) or official keyless Atom RSS feeds.
/// </summary>
public class YouTubeFeedSyncService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<YouTubeFeedSyncService> _logger;

    private static readonly Regex ChannelIdRegex = new(@"^UC[a-zA-Z0-9_-]{20,}$", RegexOptions.Compiled);
    private static readonly Regex ExternalIdJsonRegex = new(@"""(?:externalId|channelId)""\s*:\s*""(UC[a-zA-Z0-9_-]{20,})""", RegexOptions.Compiled);
    private static readonly Regex CanonicalChannelRegex = new(@"youtube\.com/channel/(UC[a-zA-Z0-9_-]{20,})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex IsoDurationRegex = new(@"^PT(?:(\d+)H)?(?:(\d+)M)?(?:(\d+)S)?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public YouTubeFeedSyncService(IHttpClientFactory httpFactory, ILogger<YouTubeFeedSyncService> logger)
    {
        _httpFactory = httpFactory;
        _logger = logger;
    }

    public async Task<string?> ResolveChannelIdAsync(string? channelUrlOrHandle, string? explicitChannelId, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(explicitChannelId) && ChannelIdRegex.IsMatch(explicitChannelId.Trim()))
            return explicitChannelId.Trim();

        if (string.IsNullOrWhiteSpace(channelUrlOrHandle)) return null;
        var raw = channelUrlOrHandle.Trim();

        var urlMatch = CanonicalChannelRegex.Match(raw);
        if (urlMatch.Success) return urlMatch.Groups[1].Value;

        string targetUrl;
        if (raw.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            targetUrl = raw;
        else if (raw.StartsWith('@'))
            targetUrl = $"https://www.youtube.com/{raw}";
        else if (ChannelIdRegex.IsMatch(raw))
            return raw;
        else
            targetUrl = $"https://www.youtube.com/@{raw}";

        try
        {
            var client = _httpFactory.CreateClient("YouTubeClient");
            using var req = new HttpRequestMessage(HttpMethod.Get, targetUrl);
            req.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (compatible; HinduPanchangBot/2.0)");
            req.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9,hi;q=0.8");
            using var res = await client.SendAsync(req, ct);
            if (!res.IsSuccessStatusCode) return null;

            var html = await res.Content.ReadAsStringAsync(ct);
            var extMatch = ExternalIdJsonRegex.Match(html);
            if (extMatch.Success) return extMatch.Groups[1].Value;

            var canMatch = CanonicalChannelRegex.Match(html);
            if (canMatch.Success) return canMatch.Groups[1].Value;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not resolve YouTube channel ID for {Target}", targetUrl);
        }

        return null;
    }

    public async Task<List<SpiritualVideo>> FetchLatestVideosAsync(
        CentralChannelConfig channel,
        string? apiKey,
        int maxResults = 8,
        CancellationToken ct = default)
    {
        var channelId = await ResolveChannelIdAsync(
            !string.IsNullOrWhiteSpace(channel.Url) ? channel.Url : channel.Handle,
            channel.ChannelId,
            ct);

        if (string.IsNullOrWhiteSpace(channelId))
        {
            _logger.LogInformation("No YouTube ChannelId resolved yet for {Name} ({Handle})", channel.Name, channel.Handle);
            return [];
        }

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            var apiVideos = await TryFetchViaYouTubeApiAsync(channel, channelId, apiKey.Trim(), maxResults, ct);
            if (apiVideos.Count > 0) return apiVideos;
        }

        return await FetchViaAtomFeedAsync(channel, channelId, maxResults, ct);
    }

    internal async Task<List<SpiritualVideo>> FetchViaAtomFeedAsync(
        CentralChannelConfig channel,
        string channelId,
        int maxResults,
        CancellationToken ct)
    {
        var feedUrl = $"https://www.youtube.com/feeds/videos.xml?channel_id={Uri.EscapeDataString(channelId)}";
        try
        {
            var client = _httpFactory.CreateClient("YouTubeClient");
            var xmlText = await client.GetStringAsync(feedUrl, ct);
            return ParseAtomXml(xmlText, channel, maxResults);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "YouTube Atom RSS fetch failed for channel {ChannelId}", channelId);
            return [];
        }
    }

    internal static List<SpiritualVideo> ParseAtomXml(string xmlText, CentralChannelConfig channel, int maxResults = 8)
    {
        var list = new List<SpiritualVideo>();
        if (string.IsNullOrWhiteSpace(xmlText)) return list;

        XNamespace atom = "http://www.w3.org/2005/Atom";
        XNamespace yt = "http://www.youtube.com/xml/schemas/2015";
        XNamespace media = "http://search.yahoo.com/mrss/";

        var doc = XDocument.Parse(xmlText);
        var entries = doc.Root?.Elements(atom + "entry").Take(Math.Max(1, maxResults)) ?? [];

        var channelUrl = !string.IsNullOrWhiteSpace(channel.Url)
            ? channel.Url
            : $"https://www.youtube.com/{(channel.Handle.StartsWith('@') ? channel.Handle : "channel/" + channel.ChannelId)}";

        foreach (var entry in entries)
        {
            var videoId = entry.Element(yt + "videoId")?.Value?.Trim();
            if (string.IsNullOrWhiteSpace(videoId)) continue;

            var title = entry.Element(atom + "title")?.Value?.Trim() ?? "Untitled Video";
            var publishedStr = entry.Element(atom + "published")?.Value;
            var published = DateTime.TryParse(publishedStr, out var dt) ? dt.ToUniversalTime() : DateTime.UtcNow;

            var group = entry.Element(media + "group");
            var description = group?.Element(media + "description")?.Value?.Trim() ?? string.Empty;
            if (description.Length > 280) description = description[..277] + "...";

            var thumbUrl = group?.Element(media + "thumbnail")?.Attribute("url")?.Value
                ?? $"https://i.ytimg.com/vi/{videoId}/hqdefault.jpg";

            long views = 0;
            var statsEl = group?.Element(media + "community")?.Element(media + "statistics");
            if (statsEl?.Attribute("views")?.Value is string vStr && long.TryParse(vStr, out var parsedViews))
                views = parsedViews;

            list.Add(new SpiritualVideo
            {
                Id = $"yt-{videoId}",
                Title = title,
                Description = string.IsNullOrWhiteSpace(description)
                    ? $"{channel.Name} • Published {published:MMM dd, yyyy}"
                    : description,
                YouTubeUrl = $"https://www.youtube.com/watch?v={videoId}",
                YouTubeVideoId = videoId,
                EmbedUrl = $"https://www.youtube.com/embed/{videoId}",
                ThumbnailUrl = thumbUrl,
                Category = InferCategory(title, description, channel.Niche),
                PublishedDate = published,
                DurationMinutes = "Video",
                ViewCount = views,
                IsFeaturedDailyVideo = false,
                ChannelName = channel.Name,
                ChannelUrl = channelUrl,
                SourceChannelHandle = channel.Handle,
                IsAutoSynced = true
            });
        }

        return list;
    }

    private async Task<List<SpiritualVideo>> TryFetchViaYouTubeApiAsync(
        CentralChannelConfig channel,
        string channelId,
        string apiKey,
        int maxResults,
        CancellationToken ct)
    {
        try
        {
            var client = _httpFactory.CreateClient("YouTubeClient");
            var chJson = await client.GetStringAsync(
                $"https://www.googleapis.com/youtube/v3/channels?part=contentDetails&id={Uri.EscapeDataString(channelId)}&key={Uri.EscapeDataString(apiKey)}",
                ct);
            using var chDoc = JsonDocument.Parse(chJson);
            var items = chDoc.RootElement.GetProperty("items");
            if (items.GetArrayLength() == 0) return [];

            var uploadsPlaylist = items[0]
                .GetProperty("contentDetails")
                .GetProperty("relatedPlaylists")
                .GetProperty("uploads")
                .GetString();
            if (string.IsNullOrWhiteSpace(uploadsPlaylist)) return [];

            var plJson = await client.GetStringAsync(
                $"https://www.googleapis.com/youtube/v3/playlistItems?part=contentDetails&playlistId={Uri.EscapeDataString(uploadsPlaylist)}&maxResults={Math.Clamp(maxResults, 1, 20)}&key={Uri.EscapeDataString(apiKey)}",
                ct);
            using var plDoc = JsonDocument.Parse(plJson);
            var ids = new List<string>();
            foreach (var item in plDoc.RootElement.GetProperty("items").EnumerateArray())
            {
                var vidId = item.GetProperty("contentDetails").GetProperty("videoId").GetString();
                if (!string.IsNullOrWhiteSpace(vidId)) ids.Add(vidId);
            }
            if (ids.Count == 0) return [];

            var vidJson = await client.GetStringAsync(
                $"https://www.googleapis.com/youtube/v3/videos?part=snippet,statistics,contentDetails&id={Uri.EscapeDataString(string.Join(',', ids))}&key={Uri.EscapeDataString(apiKey)}",
                ct);
            using var vidDoc = JsonDocument.Parse(vidJson);

            var channelUrl = !string.IsNullOrWhiteSpace(channel.Url)
                ? channel.Url
                : $"https://www.youtube.com/channel/{channelId}";

            var result = new List<SpiritualVideo>();
            foreach (var v in vidDoc.RootElement.GetProperty("items").EnumerateArray())
            {
                var id = v.GetProperty("id").GetString() ?? "";
                var snippet = v.GetProperty("snippet");
                var title = snippet.GetProperty("title").GetString() ?? "";
                var desc = snippet.TryGetProperty("description", out var dProp) ? dProp.GetString() ?? "" : "";
                if (desc.Length > 280) desc = desc[..277] + "...";

                var pubStr = snippet.TryGetProperty("publishedAt", out var pProp) ? pProp.GetString() : null;
                var published = DateTime.TryParse(pubStr, out var dt) ? dt.ToUniversalTime() : DateTime.UtcNow;

                var thumb = $"https://i.ytimg.com/vi/{id}/hqdefault.jpg";
                if (snippet.TryGetProperty("thumbnails", out var thumbs) && thumbs.TryGetProperty("high", out var hi) && hi.TryGetProperty("url", out var u))
                    thumb = u.GetString() ?? thumb;

                long views = 0;
                if (v.TryGetProperty("statistics", out var stats) && stats.TryGetProperty("viewCount", out var vc) && long.TryParse(vc.GetString(), out var parsedViews))
                    views = parsedViews;

                var isoDur = v.TryGetProperty("contentDetails", out var cd) && cd.TryGetProperty("duration", out var dur) ? dur.GetString() : null;

                result.Add(new SpiritualVideo
                {
                    Id = $"yt-{id}",
                    Title = title,
                    Description = desc,
                    YouTubeUrl = $"https://www.youtube.com/watch?v={id}",
                    YouTubeVideoId = id,
                    EmbedUrl = $"https://www.youtube.com/embed/{id}",
                    ThumbnailUrl = thumb,
                    Category = InferCategory(title, desc, channel.Niche),
                    PublishedDate = published,
                    DurationMinutes = FormatIsoDuration(isoDur),
                    ViewCount = views,
                    IsFeaturedDailyVideo = false,
                    ChannelName = channel.Name,
                    ChannelUrl = channelUrl,
                    SourceChannelHandle = channel.Handle,
                    IsAutoSynced = true
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "YouTube Data API v3 fetch failed for {Channel}; falling back to Atom RSS", channel.Name);
            return [];
        }
    }

    internal static string InferCategory(string title, string description, string? niche)
    {
        var text = $"{title} {description} {niche}".ToLowerInvariant();
        if (text.Contains("mantra") || text.Contains("chalisa") || text.Contains("stotra") || text.Contains("aarti") || text.Contains("bhajan") || text.Contains("shiv") || text.Contains("om namah") || text.Contains("meditat"))
            return "Mantra & Meditation";
        if (text.Contains("katha") || text.Contains("story") || text.Contains("ramayan") || text.Contains("mahabharat") || text.Contains("puran") || text.Contains("krishna"))
            return "Pauranik Katha";
        if (text.Contains("temple") || text.Contains("darshan") || text.Contains("jyotirlinga") || text.Contains("kashi") || text.Contains("kedarnath") || text.Contains("mandir"))
            return "Temple Darshan";
        return "Daily News & Rashifal";
    }

    internal static string FormatIsoDuration(string? iso)
    {
        if (string.IsNullOrWhiteSpace(iso)) return "Video";
        var m = IsoDurationRegex.Match(iso.Trim());
        if (!m.Success) return "Video";
        int h = int.TryParse(m.Groups[1].Value, out var hh) ? hh : 0;
        int min = int.TryParse(m.Groups[2].Value, out var mm) ? mm : 0;
        int sec = int.TryParse(m.Groups[3].Value, out var ss) ? ss : 0;
        if (h > 0) return $"{h}:{min:D2}:{sec:D2}";
        return $"{min}:{sec:D2}";
    }
}
