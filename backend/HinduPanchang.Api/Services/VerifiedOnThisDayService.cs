using System.Collections.Concurrent;
using System.Text.Json;
using HinduPanchang.Api.Models;

namespace HinduPanchang.Api.Services;

/// <summary>
/// Fetches and caches verified "On This Day" historical events for exact calendar month/day
/// from the official Wikimedia REST API, prioritizing Indian cultural, scientific &amp; heritage
/// milestones and attaching canonical verification source URLs.
/// </summary>
public class VerifiedOnThisDayService
{
    private sealed record CacheEntry(List<HistoricalIncident> Items, DateTimeOffset ExpiresAtUtc);

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly string[] IndiaFocusTerms =
    [
        "india", "indian", "delhi", "jaipur", "varanasi", "kashi", "bengal", "mumbai", "bombay",
        "kolkata", "calcutta", "chennai", "madras", "hyderabad", "mysore", "maratha", "mughal",
        "maurya", "gupta", "chola", "rajput", "sikh", "gandhi", "nehru", "bose", "patel",
        "ambedkar", "tagore", "raman", "isro", "hindustan", "bharat", "swaraj", "lok sabha"
    ];

    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(12);
    private readonly IHttpClientFactory? _httpFactory;
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new(StringComparer.Ordinal);

    public VerifiedOnThisDayService(IHttpClientFactory? httpFactory = null)
    {
        _httpFactory = httpFactory;
    }

    public static DateTime IstNow()
    {
        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        }
        catch (TimeZoneNotFoundException)
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            }
            catch
            {
                return DateTime.UtcNow.AddHours(5.5);
            }
        }
    }

    public virtual async Task<List<HistoricalIncident>> GetForMonthDayAsync(
        int month,
        int day,
        CancellationToken ct = default)
    {
        if (month is < 1 or > 12 || day is < 1 or > 31) return [];
        if (_httpFactory is null) return [];

        string key = $"{month:D2}-{day:D2}";
        var now = DateTimeOffset.UtcNow;
        if (_cache.TryGetValue(key, out var cached) && cached.ExpiresAtUtc > now)
            return cached.Items;

        try
        {
            var client = _httpFactory.CreateClient("WikiOnThisDayClient");
            using var req = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://en.wikipedia.org/api/rest_v1/feed/onthisday/all/{month:D2}/{day:D2}");
            req.Headers.TryAddWithoutValidation("User-Agent", "HinduPanchangHub/1.0 (https://panchang.keshavsingh.in)");
            req.Headers.TryAddWithoutValidation("Accept", "application/json");

            using var res = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
            if (!res.IsSuccessStatusCode)
                return cached?.Items ?? [];

            await using var stream = await res.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            var payload = await JsonSerializer.DeserializeAsync<WikiOnThisDayResponse>(stream, JsonOpts, ct).ConfigureAwait(false);
            if (payload is null) return [];

            var parsed = BuildVerifiedIncidents(key, payload);
            _cache[key] = new CacheEntry(parsed, now + CacheDuration);
            return parsed;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return cached?.Items ?? [];
        }
    }

    private static List<HistoricalIncident> BuildVerifiedIncidents(string key, WikiOnThisDayResponse payload)
    {
        var combined = new List<(HistoricalIncident Item, int Score)>();
        var seenTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void Collect(IEnumerable<WikiOnThisDayEntry>? entries, string category, int baseScore)
        {
            if (entries is null) return;
            foreach (var e in entries)
            {
                if (e.Year <= 0 || string.IsNullOrWhiteSpace(e.Text)) continue;

                var firstPage = e.Pages?.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.ContentUrls?.Desktop?.Page));
                var sourceUrl = firstPage?.ContentUrls?.Desktop?.Page;
                var extract = firstPage?.Extract;
                var rawTitle = CleanText(e.Text);
                var title = rawTitle.Length > 130 ? rawTitle[..127].TrimEnd() + "..." : rawTitle;
                if (!seenTitles.Add($"{e.Year}:{title[..Math.Min(title.Length, 40)]}")) continue;

                var combinedText = $"{title} {extract}".ToLowerInvariant();
                var indiaHits = IndiaFocusTerms.Count(term => combinedText.Contains(term, StringComparison.Ordinal));
                var score = baseScore + (indiaHits * 100);

                combined.Add((new HistoricalIncident
                {
                    Id = $"wiki-{key}-{e.Year}-{Math.Abs(title.GetHashCode(StringComparison.Ordinal)) % 100000}",
                    DateKey = key,
                    Year = e.Year,
                    Title = title,
                    Category = indiaHits > 0 ? "Indian History" : category,
                    Description = !string.IsNullOrWhiteSpace(extract) && extract.Length > title.Length
                        ? $"{rawTitle}. {extract}"
                        : rawTitle,
                    CulturalSignificance = $"Verified On This Day ({key}) via Wikipedia encyclopedic archive.",
                    IsExactDateMatch = true,
                    IsVerified = true,
                    SourceName = "Wikipedia On This Day",
                    SourceUrl = sourceUrl ?? $"https://en.wikipedia.org/wiki/{key.Replace('-', '_')}"
                }, score));
            }
        }

        Collect(payload.Selected, "World Heritage", 30);
        Collect(payload.Events, "World History", 20);
        Collect(payload.Births, "Birth Anniversary", 15);
        Collect(payload.Deaths, "Remembering", 10);

        return combined
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Item.Year)
            .Take(8)
            .Select(x => x.Item)
            .ToList();
    }

    private static string CleanText(string text) =>
        string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private sealed record WikiOnThisDayResponse(
        List<WikiOnThisDayEntry>? Selected,
        List<WikiOnThisDayEntry>? Events,
        List<WikiOnThisDayEntry>? Births,
        List<WikiOnThisDayEntry>? Deaths);

    private sealed record WikiOnThisDayEntry(
        int Year,
        string? Text,
        List<WikiPageRef>? Pages);

    private sealed record WikiPageRef(
        string? Title,
        string? Extract,
        WikiContentUrls? ContentUrls);

    private sealed record WikiContentUrls(WikiUrlPair? Desktop);
    private sealed record WikiUrlPair(string? Page);
}
